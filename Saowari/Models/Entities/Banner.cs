using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("Banner")]
    public class Banner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BannerId { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;

        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        /// <summary>
        /// e.g. "UpcomingTrips" or "PopularRoutes"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Position { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string SizeTemplate { get; set; } = "Horizontal";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
