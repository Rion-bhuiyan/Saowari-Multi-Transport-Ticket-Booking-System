using System.Collections.Generic;

namespace Saowari.Models.DTOs.Schedule
{
    /// <summary>
    /// Lifecycle-grouped response DTO — returned by GET api/schedules/lifecycle.
    /// </summary>
    public class ScheduleLifecycleDto
    {
        /// <summary>Future schedules (status: Scheduled) whose DepartureDateTime > now.</summary>
        public List<ScheduleResponseDto> Upcoming { get; set; } = new();

        /// <summary>Currently active schedules (status: Active) between departure and arrival.</summary>
        public List<ScheduleResponseDto> Ongoing { get; set; } = new();

        /// <summary>Post-arrival schedules awaiting formal expiry approval (status: Pending Expiry).</summary>
        public List<ScheduleResponseDto> PendingExpiry { get; set; } = new();

        /// <summary>Officially expired schedules (status: Expired) — the expiration archive.</summary>
        public List<ScheduleResponseDto> Expired { get; set; } = new();
    }
}
