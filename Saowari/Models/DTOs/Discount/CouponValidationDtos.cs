using System;

namespace Saowari.Models.DTOs.Discount
{
    public class CouponValidationRequestDto
    {
        public int ScheduleId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public decimal TotalTicketAmount { get; set; }
    }

    public class CouponValidationResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public int? DiscountId { get; set; }
        public decimal DiscountValue { get; set; }
        public bool IsPercentage { get; set; }
    }
}
