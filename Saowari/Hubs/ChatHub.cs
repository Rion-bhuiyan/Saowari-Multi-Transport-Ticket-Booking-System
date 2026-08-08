using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatHub(SaowariDbContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
        }

        // ── SUPPORT CHAT FUNCTIONS ───────────────────────────────────────────────────

        public async Task JoinSupportRoom(JoinRoomRequestDto request)
        {
            // Find or create SupportRoom in database
            var room = await _context.SupportRooms
                .FirstOrDefaultAsync(r => r.UserEmailOrIP == request.UserEmailOrIP && r.IsActive);

            if (room == null)
            {
                room = new SupportRoom
                {
                    UserEmailOrIP = request.UserEmailOrIP,
                    IpAddress = request.IpAddress,
                    BrowserInfo = request.BrowserInfo,
                    Geolocation = request.Geolocation,
                    IspName = request.IspName,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.SupportRooms.Add(room);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update metadata if it was a guest but now they have info, or if their IP/location changed
                bool updated = false;
                if (!string.IsNullOrEmpty(request.IpAddress) && room.IpAddress != request.IpAddress) { room.IpAddress = request.IpAddress; updated = true; }
                if (!string.IsNullOrEmpty(request.BrowserInfo) && room.BrowserInfo != request.BrowserInfo) { room.BrowserInfo = request.BrowserInfo; updated = true; }
                if (!string.IsNullOrEmpty(request.Geolocation) && room.Geolocation != request.Geolocation) { room.Geolocation = request.Geolocation; updated = true; }
                if (!string.IsNullOrEmpty(request.IspName) && room.IspName != request.IspName) { room.IspName = request.IspName; updated = true; }
                
                if (updated)
                {
                    await _context.SaveChangesAsync();
                }
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
                IpAddress = room.IpAddress,
                BrowserInfo = room.BrowserInfo,
                Geolocation = room.Geolocation,
                IspName = room.IspName,
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

            // ── AI/AUTO-RESPONDER AUTOMATION ──────────────────────────────────────────
            // If the message is from a passenger/guest, check if it's their first message in this chat session
            bool isPassengerMessage = senderId == null || !await IsAdminUser(senderId.Value);
            if (isPassengerMessage)
            {
                bool hasPreviousMessages = await _context.SupportMessages
                    .AnyAsync(m => m.RoomId == roomId && m.Id != message.Id && m.SenderName != "Saowari Assistant");

                if (!hasPreviousMessages)
                {
                    // Trigger bilingual automated replies in the background with delays
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // 1. First Welcome Message after 1.5 seconds
                            await Task.Delay(1500);
                            await SendAutoResponse(roomId, 
                                "👋 Welcome to Saowari Support! | ছাওয়ারী সাপোর্টে আপনাকে স্বাগতম! 🚌\n" +
                                "We provide ticket bookings for Bus, Launch, and Flights across Bangladesh. | আমরা বাংলাদেশ জুড়ে বাস, লঞ্চ এবং ফ্লাইটের টিকিট বুকিং সেবা প্রদান করে থাকি।\n\n" +
                                "📞 Hotline: +880 9612-SAOWARI\n" +
                                "📧 Email: support@saowari.com");

                            // 2. Second Wait Message after 3 more seconds
                            await Task.Delay(3000);
                            await SendAutoResponse(roomId, 
                                "🤖 [Saowari Assistant]: Our support agents have been notified and will join this conversation shortly. Please stay online. | " +
                                "আমাদের সাপোর্ট প্রতিনিধিকে জানানো হয়েছে এবং শীঘ্রই তিনি চ্যাটে যুক্ত হবেন। অনুগ্রহ করে লাইনেই থাকুন।\n" +
                                "Thank you for your patience! | ধন্যবাদ! 🙏");
                        }
                        catch (Exception)
                        {
                            // Avoid throwing background exceptions
                        }
                    });
                }
            }
        }

        private async Task<bool> IsAdminUser(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.UserID == userId);
            return user?.UserRole?.UserRoleName == "Admin" || user?.UserRole?.UserRoleName == "System Administrator";
        }

        private async Task SendAutoResponse(int roomId, string content)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SaowariDbContext>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

                var room = await dbContext.SupportRooms.FindAsync(roomId);
                if (room == null) return;

                var message = new SupportMessage
                {
                    RoomId = roomId,
                    SenderName = "Saowari Assistant",
                    SenderId = null,
                    Content = content,
                    MessageType = "text",
                    FileUrl = null,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                room.LastMessageAt = DateTime.UtcNow;
                dbContext.SupportMessages.Add(message);
                await dbContext.SaveChangesAsync();

                string groupName = $"Room_{roomId}";
                await hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", new SupportMessageDto
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

                await hubContext.Clients.Group("Admins").SendAsync("ReceiveLobbyMessage", new
                {
                    roomId = roomId,
                    content = content,
                    messageType = "text",
                    lastMessageAt = room.LastMessageAt
                });
            }
        }

        public async Task RegisterAdminLobby()
        {
            // Adds active admin connections to a centralized notification lobby group
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        // ── SCHEDULE GROUP CHAT FUNCTIONS ─────────────────────────────────────────────

        public async Task JoinScheduleGroup(int scheduleId, int userId, string fullName)
        {
            var isRemoved = await _context.ScheduleChatRemovedUsers
                .AnyAsync(r => r.ScheduleId == scheduleId && r.UserId == userId);
            if (isRemoved)
            {
                // Can notify the caller they are removed
                await Clients.Caller.SendAsync("ReceiveSystemMessage", "You have been removed from this chat.");
                return;
            }

            string groupName = $"Schedule_{scheduleId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Optional: Broadcast system alert that a passenger has joined
            await Clients.Group(groupName).SendAsync("ReceiveSystemMessage", $"{fullName} has joined the group chat.");
        }

        public async Task SendMessageToSchedule(int scheduleId, int senderId, string senderName, string content, string messageType, string? fileUrl)
        {
            var isRemoved = await _context.ScheduleChatRemovedUsers
                .AnyAsync(r => r.ScheduleId == scheduleId && r.UserId == senderId);
            if (isRemoved) return;

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
