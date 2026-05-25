namespace Saowari.Models.DTOs.Schedule
{
    public class ScheduleSeatClassPricingDto
    {
        public int SeatClassId { get; set; }
        public string? SeatClassName { get; set; }
        public decimal Price { get; set; }
    }
}
