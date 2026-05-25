using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("DepartureLocation")]
    public class DepartureLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartureLocationID { get; set; }

        [Required]
        public int ScheduleID { get; set; }

        [Required]
        public int LocationID { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        [ForeignKey(nameof(ScheduleID))]
        public Schedule Schedule { get; set; } = null!;

        [ForeignKey(nameof(LocationID))]
        public Location Location { get; set; } = null!;
    }
}

