namespace Saowari.Models.DTOs.Refund
{
    public class RefundResponseDto
    {
        public int RefundID { get; set; }
        public int BookingId { get; set; }
        public int PaymentId { get; set; }
        public DateTime RequestedAt { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal RefundAmount { get; set; }
        public int RefundStatusId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? RefundTransactionID { get; set; }
        public string? Remarks { get; set; }
        public bool IsRefunded { get; set; }
        public int PolicyID { get; set; }
        
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerImage { get; set; }
        public string? PaymentMethod { get; set; }
        public string? BookingCode { get; set; }
        public string? RefundStatusName { get; set; }
        public string? UpdatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool RequiresOtp { get; set; }
    }
}