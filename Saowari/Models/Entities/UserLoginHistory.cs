using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("UserLoginHistory")]
    public class UserLoginHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string DeviceName { get; set; } = null!;

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(10)]
        public string? CountryCode { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? IspName { get; set; }

        [MaxLength(500)]
        public string? Referrer { get; set; }

        [MaxLength(50)]
        public string? TrafficChannel { get; set; }

        [MaxLength(100)]
        public string? Browser { get; set; }

        public DateTime? LastActiveTime { get; set; }

        [NotMapped]
        public double SessionDurationMinutes => LastActiveTime.HasValue ? Math.Min((LastActiveTime.Value - LoginTime).TotalMinutes, 120) : 0;

        public bool IsActive { get; set; } = true;

        public virtual User User { get; set; } = null!;
    }
}
