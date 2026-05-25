namespace Saowari.Models.DTOs.SeatPricing
{
    public class SeatPricingCreateDto
    {
        public int VehicleId { get; set; }
        public int SeatClassId { get; set; }
        public decimal Price { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
}