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

        public bool IsActive { get; set; } = true;

        public virtual User User { get; set; } = null!;
    }
}
