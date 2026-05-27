using Saowari.Models.DTOs.SeatPricing;

namespace Saowari.Models.DTOs.Vehicle
{
    public class VehicleUpdateDto
    {
        public int VehicleID { get; set; }
        public int CompanyId { get; set; }
        public string VehicleName { get; set; }
        public string VehicleNumber { get; set; }
        public string EngineNumber { get; set; }
        public string EngineCC { get; set; }
        public string ChassisNumber { get; set; }
        public int VehicleTypeId { get; set; }
        public int TotalSeats { get; set; }
        public SeatLayoutConfigDto? VisualLayout { get; set; }
        public bool IsActive { get; set; }
        /// <summary>Seat class pricing entries — replaces existing pricings for this vehicle.</summary>
        public List<SeatClassPricingInputDto> SeatClassPricings { get; set; } = new();
    }
}