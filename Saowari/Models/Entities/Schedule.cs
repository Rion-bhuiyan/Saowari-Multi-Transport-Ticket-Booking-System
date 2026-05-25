using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Saowari.Models.Entities
{
    [Table("Schedule")]
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleID { get; set; }

        [ForeignKey(nameof(Route))]
        public int RouteId { get; set; }

        [ForeignKey(nameof(Vehicle))]
        public int VehicleId { get; set; }

        [ForeignKey(nameof(DriverInformtion))]
        public int DriverInformtionId { get; set; }

        [ForeignKey("Supervisor")]
        public int? SupervisorId { get; set; }

        [Required]
        public DateTime DepartureDateTime { get; set; }

        [Required]
        public DateTime ArrivalDateTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal BasePrice { get; set; }

        [Required]
        [Range(0, 100000)]
        public int AvailableSeats { get; set; }

        [Required]
        [ForeignKey("ScheduleStatus")]
        public int ScheduleStatusId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        

        public virtual DriverInformtion? DriverInformtion { get; set; }
        public virtual Route? Route { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual ScheduleStatus? ScheduleStatus { get; set; }
        public virtual Supervisor? Supervisor { get; set; }


        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<ScheduleSeatStatus> ScheduleSeatStatuses { get; set; } = new List<ScheduleSeatStatus>();
        public virtual ICollection<DepartureLocation> DepartureLocations { get; set; } = new List<DepartureLocation>();
        public virtual ICollection<ScheduleSeatClassPricing> ScheduleSeatClassPricings { get; set; } = new List<ScheduleSeatClassPricing>();
    }
}
