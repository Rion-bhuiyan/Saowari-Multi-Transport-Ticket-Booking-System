namespace Saowari.Models.DTOs.Refund
{
    public class RefundUpdateDto
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
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}