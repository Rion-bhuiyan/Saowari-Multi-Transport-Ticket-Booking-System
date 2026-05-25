using System;
using System.Collections.Generic;

namespace Saowari.Models.DTOs.Schedule
{
    /// <summary>
    /// DTO for cloning an Expired schedule to create a new live schedule.
    /// Route, Vehicle, seat pricings, and departure locations are inherited from the original.
    /// New driver, supervisor, and dates are mandatory.
    /// </summary>
    public class ScheduleCloneDto
    {
        public int OriginalScheduleId { get; set; }
        public int DriverInformtionId { get; set; }
        public int? SupervisorId { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        // Optional overrides — if null, copied from original
        public List<Saowari.Models.DTOs.Route.DepartureLocationDto>? DepartureLocations { get; set; }
        public List<ScheduleSeatClassPricingDto>? SeatClassPricings { get; set; }
    }
}
