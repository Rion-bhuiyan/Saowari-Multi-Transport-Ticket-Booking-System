using System;
using System.Collections.Generic;

namespace Saowari.Models.DTOs.Schedule
{
    public class ScheduleResponseDto
    {
        public int ScheduleID { get; set; }
        public int RouteId { get; set; }
        public string? Route { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleNumber { get; set; }
        public int DriverInformtionId { get; set; }
        public int? SupervisorId { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public decimal BasePrice { get; set; }
        public int AvailableSeats { get; set; }
        public int ScheduleStatusId { get; set; }
        public string? ScheduleStatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SeatLayoutConfig { get; set; }
        // Company info
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public List<Saowari.Models.DTOs.Route.DepartureLocationDto> DepartureLocations { get; set; } = new List<Saowari.Models.DTOs.Route.DepartureLocationDto>();
        public List<ScheduleSeatClassPricingDto> SeatClassPricings { get; set; } = new List<ScheduleSeatClassPricingDto>();
    }
}