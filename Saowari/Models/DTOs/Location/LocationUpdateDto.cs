using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Location
{
    public class LocationUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; }

        public int LocationCode { get; set; }

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal? Latitude { get; set; }

        [MaxLength(60)]
        public string? Longitude { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        public bool IsActive { get; set; } = true;
    }
}