using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;

namespace Saowari.Models.Entities
{
    [Table("Route")]
    public class Route
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RouteID { get; set; }

        [Required]
        public int FromLocationID { get; set; }

        [Required]
        public int ToLocationID { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 99999999.99)]
        public decimal? DistanceKM { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 999.99)]
        public decimal? EstimatedHours { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        [Url]
        public string? ImageUrl { get; set; }

        [ForeignKey(nameof(FromLocationID))]
        public Location FromLocation { get; set; } = null!;

        [ForeignKey(nameof(ToLocationID))]
        public Location ToLocation { get; set; } = null!;

        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }
}
