using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Target user — null means it was sent to all admins (stored per-user by the service)</summary>
        public int? UserId { get; set; }

        /// <summary>The company this notification is scoped to (null = system-wide)</summary>
        public int? CompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = null!;

        /// <summary>booking | cancellation | refund | user | system | vehicle | schedule</summary>
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "system";

        /// <summary>Entity type for deep-linking (Booking, Refund, User, Vehicle, Schedule)</summary>
        [MaxLength(50)]
        public string? EntityType { get; set; }

        /// <summary>Entity ID for deep-linking</summary>
        public int? EntityId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Icon { get; set; } = "fas fa-bell";

        [Required]
        [MaxLength(100)]
        public string ColorClass { get; set; } = "bg-blue-100 text-blue-600";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
    }
}
