using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class DriverInformtion
    {
        public int DriverInformtionId { get; set; }
        [Required, MaxLength(150)]
        public string LicenceNumber { get; set; }= null!;
        public DateTime licenceExpDate{ get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    }
}
