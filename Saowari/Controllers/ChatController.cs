using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Saowari.Hubs;
using Saowari.Data;
using Saowari.Models.Entities;
using Saowari.Models.DTOs.Chat;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly SaowariDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(SaowariDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ── SUPPORT APIS ──────────────────────────────────────────────────────────────

        public class ContactFormDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? BookingReference { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost("contact")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest("Email and Message are required.");
            }

            var room = await _context.SupportRooms
                .FirstOrDefaultAsync(r => r.UserEmailOrIP == model.Email && r.IsActive);

            if (room == null)
            {
                room = new SupportRoom
                {
                    UserEmailOrIP = model.Email,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.SupportRooms.Add(room);
                await _context.SaveChangesAsync();
                
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveRoomUpdate", new SupportRoomDto
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

            string fullMessage = $"[Contact Form Submission]\n" +
                                 $"Name: {model.Name}\n" +
                                 $"Category: {model.Category}\n" +
                                 (!string.IsNullOrEmpty(model.BookingReference) ? $"Ref: {model.BookingReference}\n" : "") +
                                 $"\n{model.Message}";

            var message = new SupportMessage
            {
                RoomId = room.Id,
                SenderName = string.IsNullOrWhiteSpace(model.Name) ? "Guest" : model.Name,
                SenderId = null,
                Content = fullMessage,
                MessageType = "text",
                FileUrl = null,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            room.LastMessageAt = DateTime.UtcNow;
            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();

            string groupName = $"Room_{room.Id}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", new SupportMessageDto
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

            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveLobbyMessage", new
            {
                roomId = room.Id,
                content = fullMessage,
                messageType = "text",
                lastMessageAt = room.LastMessageAt
            });

            return Ok(new { success = true });
        }

        [HttpGet("rooms")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetSupportRooms()
        {
            var rooms = await _context.SupportRooms
                .Include(r => r.AssignedAdmin)
                .OrderByDescending(r => r.LastMessageAt)
                .Select(r => new SupportRoomDto
                {
                    Id = r.Id,
                    UserEmailOrIP = r.UserEmailOrIP,
                    IpAddress = r.IpAddress,
                    BrowserInfo = r.BrowserInfo,
                    Geolocation = r.Geolocation,
                    IspName = r.IspName,
                    AssignedAdminId = r.AssignedAdminId,
                    AssignedAdminName = r.AssignedAdmin != null ? r.AssignedAdmin.FullName : null,
                    IsActive = r.IsActive,
                    CreatedAt = DateTime.SpecifyKind(r.CreatedAt, DateTimeKind.Utc),
                    LastMessageAt = DateTime.SpecifyKind(r.LastMessageAt, DateTimeKind.Utc),
                    UnreadCount = r.SupportMessages.Count(m => !m.IsRead && m.SenderId == null),
                    LastMessageContent = r.SupportMessages.OrderByDescending(m => m.CreatedAt).Select(m => m.Content).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpGet("rooms/{roomId}/messages")]
        public async Task<IActionResult> GetRoomMessages(int roomId)
        {
            var messages = await _context.SupportMessages
                .Where(m => m.RoomId == roomId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new SupportMessageDto
                {
                    Id = m.Id,
                    RoomId = m.RoomId,
                    SenderName = m.SenderName,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    MessageType = m.MessageType,
                    FileUrl = m.FileUrl,
                    CreatedAt = DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc),
                    IsRead = m.IsRead
                })
                .ToListAsync();

            // Auto-mark as read if admin is fetching
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Admin")
            {
                var unreadMsgs = await _context.SupportMessages
                    .Where(m => m.RoomId == roomId && !m.IsRead && m.SenderId == null)
                    .ToListAsync();
                if (unreadMsgs.Any())
                {
                    unreadMsgs.ForEach(m => m.IsRead = true);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(messages);
        }

        [HttpPost("rooms/{roomId}/claim")]
        [Authorize]
        public async Task<IActionResult> ClaimRoom(int roomId)
        {
            var userIdVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdVal) || !int.TryParse(userIdVal, out int adminId))
            {
                return Unauthorized("Invalid user token metadata.");
            }

            var room = await _context.SupportRooms.FindAsync(roomId);
            if (room == null) return NotFound("Support session room not found.");

            if (room.AssignedAdminId != null && room.AssignedAdminId != adminId)
            {
                var otherAdmin = await _context.Users.FindAsync(room.AssignedAdminId);
                return BadRequest($"Conversation is currently locked to active admin: {otherAdmin?.FullName ?? "Another Admin"}.");
            }

            room.AssignedAdminId = adminId;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, assignedAdminId = adminId });
        }

        [HttpPost("rooms/{roomId}/release")]
        [Authorize]
        public async Task<IActionResult> ReleaseRoom(int roomId)
        {
            var room = await _context.SupportRooms.FindAsync(roomId);
            if (room == null) return NotFound("Support session room not found.");

            room.AssignedAdminId = null;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpDelete("messages/{messageId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteSupportMessage(int messageId)
        {
            var message = await _context.SupportMessages.FindAsync(messageId);
            if (message == null) return NotFound("Message not found.");

            _context.SupportMessages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // ── PASSENGER ACTIVE SCHEDULE GROUPS ──────────────────────────────────────────

        [HttpGet("passenger/active-schedules")]
        [Authorize]
        public async Task<IActionResult> GetPassengerActiveSchedules()
        {
            var userIdVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdVal) || !int.TryParse(userIdVal, out int userId))
            {
                return Unauthorized();
            }

            // Fetch schedules where the user has purchased valid active tickets
            var activeSchedules = await _context.Bookings
                .Where(b => b.UserID == userId && b.BookingStatus.BookingStatusName != "Cancelled" && b.Schedule.ArrivalDateTime >= DateTime.UtcNow.AddHours(-24))
                .Select(b => b.Schedule)
                .Distinct()
                .Select(s => new {
                    ScheduleID = s.ScheduleID,
                    DepartureDateTime = DateTime.SpecifyKind(s.DepartureDateTime, DateTimeKind.Utc),
                    ArrivalDateTime = DateTime.SpecifyKind(s.ArrivalDateTime, DateTimeKind.Utc),
                    BasePrice = s.BasePrice,
                    RouteName = s.Route != null ? $"{s.Route.FromLocation.LocationName} to {s.Route.ToLocation.LocationName}" : "Unknown Route",
                    VehicleName = s.Vehicle != null ? $"{s.Vehicle.Company.CompanyName} ({s.Vehicle.VehicleNumber})" : "Unknown Vehicle",
                    VehicleType = s.Vehicle != null && s.Vehicle.VehicleType != null ? s.Vehicle.VehicleType.VehicleTypeName : "Standard"
                })
                .ToListAsync();

            return Ok(activeSchedules);
        }

        [HttpGet("schedule/{scheduleId}/messages")]
        [Authorize]
        public async Task<IActionResult> GetScheduleMessages(int scheduleId)
        {
            var userIdVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdVal) || !int.TryParse(userIdVal, out int userId))
            {
                return Unauthorized();
            }

            // Verify that passenger holds a valid ticket for this schedule
            var hasTicket = await _context.Bookings
                .AnyAsync(b => b.UserID == userId && b.ScheduleID == scheduleId && b.BookingStatus.BookingStatusName != "Cancelled");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAdmin = userRole == "Admin";

            if (!hasTicket && !isAdmin)
            {
                return Forbid("Access denied. You do not hold active tickets for this schedule.");
            }

            var messages = await _context.ScheduleChatMessages
                .Where(m => m.ScheduleId == scheduleId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ScheduleChatMessageDto
                {
                    Id = m.Id,
                    ScheduleId = m.ScheduleId,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    Content = m.Content,
                    MessageType = m.MessageType,
                    FileUrl = m.FileUrl,
                    CreatedAt = DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc)
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("schedule/{scheduleId}/members")]
        [Authorize]
        public async Task<IActionResult> GetScheduleMembers(int scheduleId)
        {
            var schedule = await _context.Schedules
                .Include(s => s.DriverInformtion).ThenInclude(d => d.Users)
                .Include(s => s.Supervisor).ThenInclude(sup => sup.Users)
                .FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);

            if (schedule == null) return NotFound("Schedule not found.");

            // Get valid passengers
            var passengers = await _context.Bookings
                .Where(b => b.ScheduleID == scheduleId && b.BookingStatus.BookingStatusName != "Cancelled")
                .Select(b => b.User)
                .Distinct()
                .ToListAsync();

            // Get removed users
            var removedUsers = await _context.ScheduleChatRemovedUsers
                .Where(r => r.ScheduleId == scheduleId)
                .ToListAsync();

            var members = new System.Collections.Generic.List<ScheduleChatMemberDto>();

            // Add Driver
            var driverUser = schedule.DriverInformtion?.Users.FirstOrDefault();
            if (driverUser != null)
            {
                members.Add(new ScheduleChatMemberDto
                {
                    UserId = driverUser.UserID,
                    FullName = driverUser.FullName,
                    Role = "Driver",
                    IsRemoved = false
                });
            }

            // Add Supervisor
            var supervisorUser = schedule.Supervisor?.Users.FirstOrDefault();
            if (supervisorUser != null)
            {
                members.Add(new ScheduleChatMemberDto
                {
                    UserId = supervisorUser.UserID,
                    FullName = supervisorUser.FullName,
                    Role = "Supervisor",
                    IsRemoved = false
                });
            }

            // Add Passengers
            foreach (var p in passengers)
            {
                if (p == null) continue;
                var removal = removedUsers.FirstOrDefault(r => r.UserId == p.UserID);
                members.Add(new ScheduleChatMemberDto
                {
                    UserId = p.UserID,
                    FullName = p.FullName,
                    Role = "Passenger",
                    IsRemoved = removal != null,
                    RemovedAt = removal?.RemovedAt
                });
            }

            return Ok(members);
        }

        [HttpDelete("schedule/{scheduleId}/members/{memberId}")]
        [Authorize(Roles = "Driver,Supervisor")]
        public async Task<IActionResult> RemoveUserFromSchedule(int scheduleId, int memberId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int adminUserId)) return Unauthorized();

            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return NotFound("Schedule not found.");

            var adminUser = await _context.Users.FindAsync(adminUserId);
            bool isDriver = schedule.DriverInformtionId != 0 && adminUser?.DriverInformtionId == schedule.DriverInformtionId;
            bool isSupervisor = schedule.SupervisorId.HasValue && adminUser?.SupervisorId == schedule.SupervisorId;

            if (!isDriver && !isSupervisor)
                return Forbid("Only the assigned Driver or Supervisor can remove members.");

            var existingRemoval = await _context.ScheduleChatRemovedUsers
                .FirstOrDefaultAsync(r => r.ScheduleId == scheduleId && r.UserId == memberId);

            if (existingRemoval == null)
            {
                _context.ScheduleChatRemovedUsers.Add(new ScheduleChatRemovedUser
                {
                    ScheduleId = scheduleId,
                    UserId = memberId,
                    RemovedByUserId = adminUserId
                });
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, message = "User removed from chat group." });
        }
    }
}
