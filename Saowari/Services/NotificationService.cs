using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Saowari.Data;
using Saowari.Hubs;
using Saowari.Interfaces;
using Saowari.Models.Entities;

namespace Saowari.Services
{
    public class NotificationService : INotificationService
    {
        private readonly SaowariDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(SaowariDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ─── Core helpers ──────────────────────────────────────────────────────

        /// <summary>Send a notification to every admin user, respecting their per-company preference.</summary>
        public async Task CreateForAdminsAsync(string title, string message, string type, string icon,
            string colorClass, int? companyId = null, string? entityType = null, int? entityId = null)
        {
            var adminRole = await _context.UserRoles
                .FirstOrDefaultAsync(r => r.UserRoleName == "Admin");
            if (adminRole == null) return;

            var admins = await _context.Users
                .Where(u => u.RoleID == adminRole.UserRoleId && u.IsActive)
                .ToListAsync();

            foreach (var admin in admins)
            {
                // If this is a company-scoped notification, respect the admin's preference
                if (companyId.HasValue)
                {
                    var pref = await _context.AdminNotificationPreferences
                        .FirstOrDefaultAsync(p => p.AdminUserId == admin.UserID && p.CompanyId == companyId.Value);

                    // Default is enabled; skip only if explicitly disabled
                    if (pref != null && !pref.IsEnabled) continue;
                }

                var notification = new Notification
                {
                    UserId = admin.UserID,
                    CompanyId = companyId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Icon = icon,
                    ColorClass = colorClass,
                    EntityType = entityType,
                    EntityId = entityId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Notifications.AddAsync(notification);
                
                await _hubContext.Clients.User(admin.UserID.ToString()).SendAsync("ReceiveNotification", new
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    CompanyId = notification.CompanyId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Icon = notification.Icon,
                    ColorClass = notification.ColorClass,
                    EntityType = notification.EntityType,
                    EntityId = notification.EntityId,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                });
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>Send a notification to all Agent users belonging to a specific company.</summary>
        public async Task CreateForCompanyAgentsAsync(int companyId, string title, string message,
            string type, string icon, string colorClass, string? entityType = null, int? entityId = null)
        {
            var agentRole = await _context.UserRoles
                .FirstOrDefaultAsync(r => r.UserRoleName == "Agent");
            if (agentRole == null) return;

            var agents = await _context.Users
                .Where(u => u.RoleID == agentRole.UserRoleId && u.CompanyId == companyId && u.IsActive)
                .ToListAsync();

            foreach (var agent in agents)
            {
                var notification = new Notification
                {
                    UserId = agent.UserID,
                    CompanyId = companyId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Icon = icon,
                    ColorClass = colorClass,
                    EntityType = entityType,
                    EntityId = entityId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Notifications.AddAsync(notification);
                
                await _hubContext.Clients.User(agent.UserID.ToString()).SendAsync("ReceiveNotification", new
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    CompanyId = notification.CompanyId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Icon = notification.Icon,
                    ColorClass = notification.ColorClass,
                    EntityType = notification.EntityType,
                    EntityId = notification.EntityId,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                });
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>Send to both company agents AND admins (admins respect company preference).</summary>
        public async Task CreateForBothAsync(int companyId, string title, string message, string type,
            string icon, string colorClass, string? entityType = null, int? entityId = null)
        {
            await CreateForCompanyAgentsAsync(companyId, title, message, type, icon, colorClass, entityType, entityId);
            await CreateForAdminsAsync(title, message, type, icon, colorClass, companyId, entityType, entityId);
        }

        /// <summary>Send a real-time notification to a specific user (e.g. passenger/customer).</summary>
        public async Task CreateForUserAsync(int userId, string title, string message, string type, string icon, string colorClass, string? entityType = null, int? entityId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Icon = icon,
                ColorClass = colorClass,
                EntityType = entityType,
                EntityId = entityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
            {
                Id = notification.Id,
                UserId = notification.UserId,
                CompanyId = notification.CompanyId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Icon = notification.Icon,
                ColorClass = notification.ColorClass,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            });
        }

        // ─── Domain-specific triggers ──────────────────────────────────────────

        public async Task NotifyBookingCreatedAsync(Booking booking)
        {
            // Load related data needed for message
            var schedule = booking.Schedule ?? await _context.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v!.Company)
                .Include(s => s.Route).ThenInclude(r => r!.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r!.ToLocation)
                .FirstOrDefaultAsync(s => s.ScheduleID == booking.ScheduleID);

            var companyId = schedule?.Vehicle?.CompanyId;
            var companyName = schedule?.Vehicle?.Company?.CompanyName ?? "Unknown Company";
            var route = schedule?.Route != null
                ? $"{schedule.Route.FromLocation?.LocationName} → {schedule.Route.ToLocation?.LocationName}"
                : "Unknown Route";

            var title = "New Ticket Booked";
            var message = $"Booking #{booking.BookingCode} by {booking.PassengerName} | Route: {route} | Amount: ৳{booking.FinalAmount:N0}";

            if (companyId.HasValue)
            {
                await CreateForBothAsync(companyId.Value, title, message,
                    "booking", "fas fa-ticket-alt", "bg-green-100 text-green-600",
                    "Booking", booking.BookingID);
            }
            else
            {
                await CreateForAdminsAsync(title, message,
                    "booking", "fas fa-ticket-alt", "bg-green-100 text-green-600",
                    null, "Booking", booking.BookingID);
            }

            // Also notify the customer!
            if (booking.UserID > 0)
            {
                await CreateForUserAsync(booking.UserID, 
                    "Ticket Purchased", 
                    $"Your ticket for {route} has been successfully booked (Booking #{booking.BookingCode}).",
                    "booking", "fas fa-ticket-alt", "bg-blue-100 text-blue-600",
                    "Booking", booking.BookingID);
            }
        }

        public async Task NotifyBookingCancelledAsync(Booking booking)
        {
            var schedule = booking.Schedule ?? await _context.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v!.Company)
                .FirstOrDefaultAsync(s => s.ScheduleID == booking.ScheduleID);

            var companyId = schedule?.Vehicle?.CompanyId;
            var title = "Booking Cancelled";
            var message = $"Booking #{booking.BookingCode} for {booking.PassengerName} has been cancelled.";

            if (companyId.HasValue)
            {
                await CreateForBothAsync(companyId.Value, title, message,
                    "cancellation", "fas fa-times-circle", "bg-red-100 text-red-600",
                    "Booking", booking.BookingID);
            }
            else
            {
                await CreateForAdminsAsync(title, message,
                    "cancellation", "fas fa-times-circle", "bg-red-100 text-red-600",
                    null, "Booking", booking.BookingID);
            }

            if (booking.UserID > 0)
            {
                await CreateForUserAsync(booking.UserID, 
                    "Booking Cancelled", 
                    $"Your booking #{booking.BookingCode} has been cancelled.",
                    "cancellation", "fas fa-times-circle", "bg-red-100 text-red-600",
                    "Booking", booking.BookingID);
            }
        }

        public async Task NotifyRefundRequestedAsync(Refund refund)
        {
            var booking = refund.Booking ?? await _context.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s!.Vehicle).ThenInclude(v => v!.Company)
                .FirstOrDefaultAsync(b => b.BookingID == refund.BookingId);

            var companyId = booking?.Schedule?.Vehicle?.CompanyId;
            var title = "Refund Requested";
            var message = $"Refund of ৳{refund.RefundAmount:N0} requested for booking #{booking?.BookingCode ?? refund.BookingId.ToString()}.";

            if (companyId.HasValue)
            {
                await CreateForBothAsync(companyId.Value, title, message,
                    "refund", "fas fa-undo-alt", "bg-yellow-100 text-yellow-600",
                    "Refund", refund.RefundID);
            }
            else
            {
                await CreateForAdminsAsync(title, message,
                    "refund", "fas fa-undo-alt", "bg-yellow-100 text-yellow-600",
                    null, "Refund", refund.RefundID);
            }

            if (booking != null && booking.UserID > 0)
            {
                await CreateForUserAsync(booking.UserID, 
                    "Refund Requested", 
                    $"We have received your refund request for booking #{booking.BookingCode}.",
                    "refund", "fas fa-undo-alt", "bg-yellow-100 text-yellow-600",
                    "Refund", refund.RefundID);
            }
        }

        public async Task NotifyRefundProcessedAsync(Refund refund)
        {
            var booking = refund.Booking ?? await _context.Bookings
                .Include(b => b.Schedule).ThenInclude(s => s!.Vehicle).ThenInclude(v => v!.Company)
                .FirstOrDefaultAsync(b => b.BookingID == refund.BookingId);

            var companyId = booking?.Schedule?.Vehicle?.CompanyId;
            
            var statusName = refund.RefundStatus?.StatusName;
            if (string.IsNullOrEmpty(statusName))
            {
                var statusObj = await _context.RefundStatuses.FindAsync(refund.RefundStatusId);
                statusName = statusObj?.StatusName;
            }
            if (string.IsNullOrEmpty(statusName)) statusName = "Updated";

            var title = $"Refund {statusName}";
            var message = $"Refund of ৳{refund.RefundAmount:N0} for booking #{booking?.BookingCode ?? refund.BookingId.ToString()} has been {statusName.ToLower()}.";

            if (companyId.HasValue)
            {
                await CreateForBothAsync(companyId.Value, title, message,
                    "refund", "fas fa-check-circle", "bg-purple-100 text-purple-600",
                    "Refund", refund.RefundID);
            }
            else
            {
                await CreateForAdminsAsync(title, message,
                    "refund", "fas fa-check-circle", "bg-purple-100 text-purple-600",
                    null, "Refund", refund.RefundID);
            }

            if (booking != null && booking.UserID > 0)
            {
                await CreateForUserAsync(booking.UserID, 
                    title, 
                    $"Your refund for booking #{booking.BookingCode} has been {statusName.ToLower()}.",
                    "refund", "fas fa-check-circle", "bg-purple-100 text-purple-600",
                    "Refund", refund.RefundID);
            }
        }

        public async Task NotifyNewUserRegisteredAsync(User user)
        {
            var roleName = user.UserRole?.UserRoleName ?? "User";
            await CreateForAdminsAsync(
                "New User Registered",
                $"{user.FullName} ({user.Email}) has registered as {roleName}.",
                "user", "fas fa-user-plus", "bg-blue-100 text-blue-600",
                null, "User", user.UserID);
        }

        public async Task NotifyUserChangedAsync(User user, string changeDescription)
        {
            await CreateForAdminsAsync(
                "User Profile Updated",
                $"{user.FullName} ({user.Email}): {changeDescription}",
                "user", "fas fa-user-edit", "bg-indigo-100 text-indigo-600",
                null, "User", user.UserID);
        }

        public async Task NotifyVehicleChangedAsync(Vehicle vehicle, string action)
        {
            if (vehicle.CompanyId == 0) return;

            var title = $"Vehicle {action}";
            var message = $"Vehicle '{vehicle.VehicleName}' ({vehicle.VehicleNumber}) has been {action.ToLower()}.";

            await CreateForBothAsync(vehicle.CompanyId, title, message,
                "vehicle", "fas fa-bus", "bg-teal-100 text-teal-600",
                "Vehicle", vehicle.VehicleID);
        }

        public async Task NotifyScheduleChangedAsync(Schedule schedule, string action)
        {
            var vehicle = schedule.Vehicle ?? await _context.Vehicles
                .Include(v => v.Company)
                .FirstOrDefaultAsync(v => v.VehicleID == schedule.VehicleId);

            if (vehicle == null || vehicle.CompanyId == 0)
            {
                await CreateForAdminsAsync($"Schedule {action}",
                    $"Schedule #{schedule.ScheduleID} has been {action.ToLower()}.",
                    "schedule", "fas fa-calendar-alt", "bg-orange-100 text-orange-600",
                    null, "Schedule", schedule.ScheduleID);
                return;
            }

            await CreateForBothAsync(vehicle.CompanyId,
                $"Schedule {action}",
                $"Schedule #{schedule.ScheduleID} for '{vehicle.VehicleName}' has been {action.ToLower()}.",
                "schedule", "fas fa-calendar-alt", "bg-orange-100 text-orange-600",
                "Schedule", schedule.ScheduleID);
        }

        public async Task NotifySystemEventAsync(string title, string message)
        {
            await CreateForAdminsAsync(title, message, "system",
                "fas fa-cog", "bg-gray-100 text-gray-600");
        }

        public async Task NotifyBookingCancellationOtpAsync(Booking booking, string otp)
        {
            if (booking.UserID > 0)
            {
                await CreateForUserAsync(booking.UserID, 
                    "Cancellation OTP", 
                    $"Your OTP to cancel booking #{booking.BookingCode} is {otp}.",
                    "cancellation", "fas fa-key", "bg-orange-100 text-orange-600",
                    "Booking", booking.BookingID);
            }
        }
    }
}

