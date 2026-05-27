using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.Text.Json.Serialization;

namespace Saowari.Models.Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Phone]
        public string Phone { get; set; } = null!;

        [MaxLength(500)]
        public string? Picture { get; set; }

        [Required]
        [MaxLength(500)]
        [JsonIgnore]
        public string PasswordHash { get; set; } = null!;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpireTime { get; set; }
        [Required]
        [ForeignKey("UserRole")]
        public int RoleID { get; set; }

        [ForeignKey("DriverInformtion")]
        public int? DriverInformtionId { get; set; }

        [ForeignKey("Supervisor")]
        public int? SupervisorId { get; set; }

        [ForeignKey(nameof(Company))]
        public int? CompanyId { get; set; }

        public bool IsActive { get; set; } = true;

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
        
        [MaxLength(6)]
        public string? OtpCode { get; set; }
        public DateTime? OtpExpireTime { get; set; }

        // OTP for new-device login verification (separate from account-lock OTP)
        [MaxLength(6)]
        public string? LoginOtpCode { get; set; }
        public DateTime? LoginOtpExpireTime { get; set; }

        [MaxLength(6)]
        public string? EmailChangeOtpCode { get; set; }
        public DateTime? EmailChangeOtpExpireTime { get; set; }

        [MaxLength(100)]
        public string? PendingNewEmail { get; set; }

        [MaxLength(150)]
        [EmailAddress]
        public string? AdminCopyEmail { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        [MaxLength(6)]
        public string? RegistrationOtpCode { get; set; }
        public DateTime? RegistrationOtpExpireTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Company? Company { get; set; }
        public virtual Supervisor? Supervisor { get; set; }
        public virtual UserRole? UserRole { get; set; }
        public virtual DriverInformtion? DriverInformtion { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
