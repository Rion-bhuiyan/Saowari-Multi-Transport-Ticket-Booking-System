using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    /// <summary>
    /// Per-schedule seat class pricing override.
    /// When a schedule is created the vehicle's default SeatPricings are copied here.
    /// Managers may override prices per schedule.
    /// </summary>
    [Table("ScheduleSeatClassPricing")]
    public class ScheduleSeatClassPricing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Schedule))]
        public int ScheduleId { get; set; }

        [Required]
        [ForeignKey(nameof(SeatClass))]
        public int SeatClassId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal Price { get; set; }

        public virtual Schedule? Schedule { get; set; }
        public virtual SeatClass? SeatClass { get; set; }
    }
}
