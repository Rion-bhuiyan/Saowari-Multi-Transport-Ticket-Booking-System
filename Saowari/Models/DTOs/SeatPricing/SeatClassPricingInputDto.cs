namespace Saowari.Models.DTOs.SeatPricing
{
    /// <summary>Lightweight input DTO used when setting seat class pricing for a vehicle.</summary>
    public class SeatClassPricingInputDto
    {
        public int SeatClassId { get; set; }
        public decimal Price { get; set; }
    }
}
