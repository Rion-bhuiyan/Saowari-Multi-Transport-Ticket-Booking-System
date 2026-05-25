namespace Saowari.Models.DTOs.BookingSeat
{
    public class BookingSeatUpdateDto
    {
        public int BookingSeatId { get; set; }
        public int BookingId { get; set; }
        public int SeatId { get; set; }
    }
}