namespace Saowari.Models.DTOs.Payment
{
    public class PaymentCreateDto
    {
        public int PaymentID { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public int PaymentMethodId { get; set; }
        public string? TransactionID { get; set; }
        public int PaymentStatusId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}