using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class Supervisor
    {
        public int SupervisorId { get; set; }
        
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
