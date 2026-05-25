using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("Location")]
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LocationID { get; set; }

        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; } = null!;


        public int LocationCode { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal? Latitude { get; set; } 

        [MaxLength(60)]
        public string? Longitude { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Route> DepartureRoutes { get; set; } = new List<Route>();
        public virtual ICollection<Route> ArrivalRoutes { get; set; } = new List<Route>();

    }
}
