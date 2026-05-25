using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class SeatClass
    {
        public int SeatClassId { get; set; }
        [Required, StringLength(30)]
        public string SeatClassName { get; set; } = default!;
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public virtual ICollection<SeatPricing> SeatPricings { get; set; } = new List<SeatPricing>();
        public  virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
