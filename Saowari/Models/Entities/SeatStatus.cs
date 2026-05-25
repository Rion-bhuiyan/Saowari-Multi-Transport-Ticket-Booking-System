using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class SeatStatus
    {
        public int SeatStatusId { get; set; }
        [Required,StringLength(50)]
        public string StatusName { get; set; } = null!;

        public virtual ICollection<ScheduleSeatStatus> ScheduleSeatStatuses { get; set; } = new List<ScheduleSeatStatus>();
    }
}
