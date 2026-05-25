namespace Saowari.Models.DTOs.PaymentMethod
{
    public class PaymentMethodResponseDto
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = null!;
        public decimal ProcessingFeePercent { get; set; }
        public decimal VATPercent { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}