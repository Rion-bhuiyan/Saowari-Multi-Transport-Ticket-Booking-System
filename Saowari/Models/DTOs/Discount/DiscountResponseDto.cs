namespace Saowari.Models.DTOs.Discount
{
    public class DiscountResponseDto
    {
        public int DiscountID { get; set; }
        public int CompanyId { get; set; }
        public int? RouteId { get; set; }
        public int? VehicleTypeId { get; set; }
        public string DiscountName { get; set; }
        public string? CouponCode { get; set; }
        public int DiscountTypeId { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinTicketAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}