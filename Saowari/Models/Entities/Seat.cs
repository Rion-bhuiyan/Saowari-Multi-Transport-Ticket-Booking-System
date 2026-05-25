using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("Seat")]
    public class Seat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SeatID { get; set; }

        [ForeignKey(nameof(Vehicle))]
        public int VehicleId { get; set; }

        [Required]
        [MaxLength(10)]
        public string SeatNumber { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal SeatPriceing { get; set; }

        [Required]
        [ForeignKey(nameof(SeatClass))]
        public int SeatClassId { get; set; } 

        public bool IsActive { get; set; } = true;

        public virtual SeatClass? SeatClass { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public ICollection<ScheduleSeatStatus> ScheduleSeatStatuses { get; set; } = new List<ScheduleSeatStatus>();
    }
}
