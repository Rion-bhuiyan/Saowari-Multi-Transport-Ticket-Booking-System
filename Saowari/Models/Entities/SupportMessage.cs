using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("SupportMessage")]
    public class SupportMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [MaxLength(150)]
        public string SenderName { get; set; } = null!;

        [ForeignKey("Sender")]
        public int? SenderId { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string MessageType { get; set; } = "text"; // text, image, video, pdf, word, voice

        [MaxLength(500)]
        public string? FileUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        [ForeignKey("RoomId")]
        public virtual SupportRoom SupportRoom { get; set; } = null!;

        public virtual User? Sender { get; set; }
    }
}
