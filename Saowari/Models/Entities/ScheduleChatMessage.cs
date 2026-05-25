using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("ScheduleChatMessage")]
    public class ScheduleChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        [MaxLength(150)]
        public string SenderName { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string MessageType { get; set; } = "text"; // text, image, video, pdf, word, voice

        [MaxLength(500)]
        public string? FileUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;
    }
}
