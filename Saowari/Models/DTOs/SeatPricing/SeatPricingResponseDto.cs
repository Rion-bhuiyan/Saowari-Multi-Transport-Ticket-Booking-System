namespace Saowari.Models.DTOs.SeatPricing
{
    public class SeatPricingResponseDto
    {
        public int PricingID { get; set; }
        public int VehicleId { get; set; }
        public int SeatClassId { get; set; }
        public string? SeatClassName { get; set; }
        public decimal Price { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}