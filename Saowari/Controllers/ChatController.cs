using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public ChatController(SaowariDbContext context)
        {
            _context = context;
        }

        // ── SUPPORT APIS ──────────────────────────────────────────────────────────────

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
    }
}
