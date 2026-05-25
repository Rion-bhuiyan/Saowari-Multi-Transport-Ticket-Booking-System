namespace Saowari.Models.DTOs.Booking
{
    public class BookingCreateDto
    {
        public int BookingID { get; set; }
        public string? BookingCode { get; set; }
        public int UserID { get; set; }
        public int ScheduleID { get; set; }
        public string? PassengerName { get; set; }
        public string? PassengerPhone { get; set; }
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

        // Added for frontend booking flow integration
        public List<int> SeatIds { get; set; } = new List<int>();
        public List<PassengerInfoDto> Passengers { get; set; } = new List<PassengerInfoDto>();
        public string? PaymentMethod { get; set; }
        public string? MobileNumber { get; set; }
        public string? TransactionId { get; set; }
        public string? BoardingPoint { get; set; }
    }

    public class PassengerInfoDto
    {
        public int SeatId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Gender { get; set; } = "Male";
        public string MobileNumber { get; set; } = string.Empty;
    }
}