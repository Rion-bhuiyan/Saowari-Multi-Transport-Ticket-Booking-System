namespace Saowari.Models.DTOs.Booking
{
    public class BookingUpdateDto
    {
        public int BookingID { get; set; }
        public string BookingCode { get; set; }
        public int UserID { get; set; }
        public int ScheduleID { get; set; }
        public string PassengerName { get; set; }
        public string PassengerPhone { get; set; }
        public string? PassengerNID { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int? DiscountID { get; set; }
        public int? BookingStatusId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public int SeatClassId { get; set; }
    }
}