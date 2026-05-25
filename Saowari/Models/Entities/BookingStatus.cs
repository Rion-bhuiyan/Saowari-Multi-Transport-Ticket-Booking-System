using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class BookingStatus
    {
        public int BookingStatusId { get; set; }
        [Required, MaxLength(50)]
        public string BookingStatusName { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
