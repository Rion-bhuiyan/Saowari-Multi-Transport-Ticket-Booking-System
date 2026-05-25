using System;

namespace Saowari.Models.DTOs.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string Icon { get; set; } = null!;
        public string ColorClass { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminNotificationPreferenceDto
    {
        public int Id { get; set; }
        public int AdminUserId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public bool IsEnabled { get; set; }
    }

    public class UpdateNotificationPreferenceDto
    {
        public bool IsEnabled { get; set; }
    }
}
