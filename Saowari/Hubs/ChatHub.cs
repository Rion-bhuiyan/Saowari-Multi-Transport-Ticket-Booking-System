using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Entities;
using Saowari.Models.DTOs.Chat;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SaowariDbContext _context;

        public ChatHub(SaowariDbContext context)
        {
            _context = context;
        }

        // ── SUPPORT CHAT FUNCTIONS ───────────────────────────────────────────────────

        public async Task JoinSupportRoom(string userEmailOrIP)
        {
            // Find or create SupportRoom in database
            var room = await _context.SupportRooms
                .FirstOrDefaultAsync(r => r.UserEmailOrIP == userEmailOrIP && r.IsActive);

            if (room == null)
            {
                room = new SupportRoom
                {
                    UserEmailOrIP = userEmailOrIP,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.SupportRooms.Add(room);
                await _context.SaveChangesAsync();
            }

            // Add client connection to the SignalR room group
            string groupName = $"Room_{room.Id}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Broadcast back room ID to the client
            await Clients.Caller.SendAsync("RoomJoined", room.Id);

            // Notify admins that an active support room list update is needed
            await Clients.Group("Admins").SendAsync("ReceiveRoomUpdate", new SupportRoomDto
            {
                Id = room.Id,
                UserEmailOrIP = room.UserEmailOrIP,
                AssignedAdminId = room.AssignedAdminId,
                IsActive = room.IsActive,
                CreatedAt = room.CreatedAt,
                LastMessageAt = room.LastMessageAt,
                UnreadCount = 0
            });
        }

        public async Task AdminJoinRoom(int roomId, int adminId, string adminName)
        {
            var room = await _context.SupportRooms.FindAsync(roomId);
            if (room == null) return;

            // If not assigned yet, lock it to this admin
            if (room.AssignedAdminId == null)
            {
                room.AssignedAdminId = adminId;
                await _context.SaveChangesAsync();
            }

            // Join the SignalR group for this support room
            string groupName = $"Room_{roomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Notify all clients in the group (especially the user) that the admin has joined
            await Clients.Group(groupName).SendAsync("AdminPresence", new { isPresent = true, adminName = adminName, adminId = adminId });

            // Notify all admins of the locked assignment status update
            await Clients.Group("Admins").SendAsync("RoomAssigned", new { roomId = roomId, adminId = adminId, adminName = adminName });
        }

        public async Task AdminLeaveRoom(int roomId)
        {
            var room = await _context.SupportRooms.FindAsync(roomId);
            if (room == null) return;

            int? formerAdminId = room.AssignedAdminId;
            room.AssignedAdminId = null;
            await _context.SaveChangesAsync();

            string groupName = $"Room_{roomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            // Notify room that admin has released/left
            await Clients.Group(groupName).SendAsync("AdminPresence", new { isPresent = false });

            // Notify all admins that the room is now free
            await Clients.Group("Admins").SendAsync("RoomReleased", new { roomId = roomId });
        }

        public async Task SendMessageToRoom(int roomId, string senderName, int? senderId, string content, string messageType, string? fileUrl)
        {
            var room = await _context.SupportRooms.FindAsync(roomId);
            if (room == null) return;

            var message = new SupportMessage
            {
                RoomId = roomId,
                SenderName = senderName,
                SenderId = senderId,
                Content = content,
                MessageType = messageType,
                FileUrl = fileUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            room.LastMessageAt = DateTime.UtcNow;
            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast message to everyone in the room (user + assigned admin)
            string groupName = $"Room_{roomId}";
            await Clients.Group(groupName).SendAsync("ReceiveMessage", new SupportMessageDto
            {
                Id = message.Id,
                RoomId = message.RoomId,
                SenderName = message.SenderName,
                SenderId = message.SenderId,
                Content = message.Content,
                MessageType = message.MessageType,
                FileUrl = message.FileUrl,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead
            });

            // Update admins lobby list with last message details
            await Clients.Group("Admins").SendAsync("ReceiveLobbyMessage", new {
                roomId = roomId,
                content = content,
                messageType = messageType,
                lastMessageAt = room.LastMessageAt
            });
        }

        public async Task RegisterAdminLobby()
        {
            // Adds active admin connections to a centralized notification lobby group
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        // ── SCHEDULE GROUP CHAT FUNCTIONS ─────────────────────────────────────────────

        public async Task JoinScheduleGroup(int scheduleId, int userId, string fullName)
        {
            string groupName = $"Schedule_{scheduleId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Optional: Broadcast system alert that a passenger has joined
            await Clients.Group(groupName).SendAsync("ReceiveSystemMessage", $"{fullName} has joined the group chat.");
        }

        public async Task SendMessageToSchedule(int scheduleId, int senderId, string senderName, string content, string messageType, string? fileUrl)
        {
            // Verify if schedule exists
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return;

            var msg = new ScheduleChatMessage
            {
                ScheduleId = scheduleId,
                SenderId = senderId,
                SenderName = senderName,
                Content = content,
                MessageType = messageType,
                FileUrl = fileUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.ScheduleChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            string groupName = $"Schedule_{scheduleId}";
            await Clients.Group(groupName).SendAsync("ReceiveScheduleMessage", new ScheduleChatMessageDto
            {
                Id = msg.Id,
                ScheduleId = msg.ScheduleId,
                SenderId = msg.SenderId,
                SenderName = msg.SenderName,
                Content = msg.Content,
                MessageType = msg.MessageType,
                FileUrl = msg.FileUrl,
                CreatedAt = msg.CreatedAt
            });
        }
    }
}
