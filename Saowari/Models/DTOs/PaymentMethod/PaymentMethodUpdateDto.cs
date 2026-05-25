using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Saowari.Models.DTOs.PaymentMethod
{
    public class PaymentMethodUpdateDto
    {
        [Required, StringLength(50)]
        public string PaymentMethodName { get; set; } = null!;

        [Range(0, 100)]
        public decimal ProcessingFeePercent { get; set; } = 0;

        [Range(0, 100)]
        public decimal VATPercent { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public IFormFile? LogoFile { get; set; }
        public string? LogoUrl { get; set; }
    }
}