using Saowari.Models.Entities;

namespace Saowari.Interfaces
{
    public interface INotificationService
    {
        // Generic creation helpers
        Task CreateForAdminsAsync(string title, string message, string type, string icon, string colorClass,
            int? companyId = null, string? entityType = null, int? entityId = null);

        Task CreateForCompanyAgentsAsync(int companyId, string title, string message, string type,
            string icon, string colorClass, string? entityType = null, int? entityId = null);

        Task CreateForBothAsync(int companyId, string title, string message, string type,
            string icon, string colorClass, string? entityType = null, int? entityId = null);

        // Domain-specific notification triggers
        Task NotifyBookingCreatedAsync(Booking booking);
        Task NotifyBookingCancelledAsync(Booking booking);
        Task NotifyRefundRequestedAsync(Refund refund);
        Task NotifyRefundProcessedAsync(Refund refund);
        Task NotifyNewUserRegisteredAsync(User user);
        Task NotifyUserChangedAsync(User user, string changeDescription);
        Task NotifyVehicleChangedAsync(Vehicle vehicle, string action);
        Task NotifyScheduleChangedAsync(Schedule schedule, string action);
        Task NotifySystemEventAsync(string title, string message);
    }
}
