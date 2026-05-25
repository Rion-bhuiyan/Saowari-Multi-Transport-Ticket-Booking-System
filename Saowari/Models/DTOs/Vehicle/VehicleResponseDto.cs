using Saowari.Models.DTOs.SeatPricing;

namespace Saowari.Models.DTOs.Vehicle
{
    public class VehicleResponseDto
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
        public string? SeatLayoutConfig { get; set; }
        public bool IsActive { get; set; }
        public List<SeatPricingResponseDto> SeatClassPricings { get; set; } = new();
        public List<Saowari.Models.DTOs.Seat.SeatResponseDto> Seats { get; set; } = new();
    }
}