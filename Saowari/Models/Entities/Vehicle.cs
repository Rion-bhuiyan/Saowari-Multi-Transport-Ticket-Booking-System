using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;

namespace Saowari.Models.Entities
{
    [Table("Vehicle")]
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VehicleID { get; set; }

        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string VehicleName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string VehicleNumber { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string EngineNumber { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string EngineCC { get; set; } = null!;

        [MaxLength(50)]
        public string ChassisNumber { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(VehicleType))]
        public int VehicleTypeId { get; set; }

        [Required]
        [Range(1, 10000)]
        public int TotalSeats { get; set; }

        public string? SeatLayoutConfig { get; set; }

        public bool IsActive { get; set; } = true;

        

        public virtual VehicleType? VehicleType { get; set; }
        public virtual Company? Company { get; set; }

        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<SeatPricing> SeatPricings { get; set; } = new List<SeatPricing>();
    }
}
