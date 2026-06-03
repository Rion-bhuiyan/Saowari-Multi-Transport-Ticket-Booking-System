using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("ScheduleChatRemovedUser")]
    public class ScheduleChatRemovedUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Schedule))]
        public int ScheduleId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(RemovedByUser))]
        public int? RemovedByUserId { get; set; }

        public DateTime RemovedAt { get; set; } = DateTime.UtcNow;

        public virtual Schedule Schedule { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual User? RemovedByUser { get; set; }
    }
}
