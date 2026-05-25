using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("ScheduleSeatStatus")]
    public class ScheduleSeatStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatusID { get; set; }

        [Required]
        [ForeignKey(nameof(Schedule))]
        public int ScheduleID { get; set; }

        [Required]
        [ForeignKey(nameof(Seat))]
        public int SeatID { get; set; }

        [ForeignKey(nameof(Booking))]
        public int? BookingID { get; set; }

        [Required]
        [ForeignKey(nameof(SeatStatus))]
        public int SeatStatusId { get; set; }
        
        public virtual Schedule? Schedule { get; set; } 
        public virtual Seat? Seat { get; set; } 
        public virtual Booking? Booking { get; set; }
        public virtual SeatStatus? SeatStatus { get; set; }
    }
}
