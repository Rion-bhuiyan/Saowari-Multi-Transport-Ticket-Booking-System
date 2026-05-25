using System;
using System.Collections.Generic;

namespace Saowari.Models.DTOs.Schedule
{
    public class ScheduleCreateDto
    {
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public int DriverInformtionId { get; set; }
        public int? SupervisorId { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public decimal BasePrice { get; set; }
        public int AvailableSeats { get; set; }
        public int ScheduleStatusId { get; set; }
        public List<Saowari.Models.DTOs.Route.DepartureLocationDto>? DepartureLocations { get; set; } = new List<Saowari.Models.DTOs.Route.DepartureLocationDto>();
        public List<ScheduleSeatClassPricingDto> SeatClassPricings { get; set; } = new List<ScheduleSeatClassPricingDto>();
    }
}