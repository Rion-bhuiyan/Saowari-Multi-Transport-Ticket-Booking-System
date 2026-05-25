namespace Saowari.Models.DTOs.PaymentCancel
{
    public class PaymentCancelCreateDto
    {
        public int PaymentCancelId { get; set; }
        public string VerificationCode { get; set; }
        public int PaymentId { get; set; }
    }
}