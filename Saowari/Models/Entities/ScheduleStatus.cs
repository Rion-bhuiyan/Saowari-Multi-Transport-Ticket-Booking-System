using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class ScheduleStatus
    {
        public int ScheduleStatusId { get; set; }
        [Required,StringLength(20)]
        public string ScheduleStatusName { get; set; } = null!;
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
