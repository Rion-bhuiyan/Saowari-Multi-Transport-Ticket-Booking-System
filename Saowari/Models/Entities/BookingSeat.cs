using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    public class BookingSeat
    {
        public int BookingSeatId { get; set; }
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        [ForeignKey("Seat")]
        public int SeatId { get; set; }

        public virtual Booking? Booking { get; set; }
        public virtual Seat? Seat { get; set; }
    }
}
