using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("Ticket")]
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketID { get; set; }

        [Required]
        [ForeignKey(nameof(Booking))]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(30)]
        public string TicketCode { get; set; } = null!;

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        public virtual Booking? Booking { get; set; }

    }
}
