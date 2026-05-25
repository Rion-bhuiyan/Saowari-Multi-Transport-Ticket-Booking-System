namespace Saowari.Models.DTOs.Booking
{
    public class BookingResponseDto
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
        public DateTime? DepartureDateTime { get; set; }
        public string? BookingStatus { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public string? VehicleName { get; set; }
        public int NumberOfSeats { get; set; }
        public string? SeatNumbers { get; set; }
    }
}