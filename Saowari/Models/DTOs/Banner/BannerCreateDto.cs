using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Banner
{
    public class BannerCreateDto
    {
        public string? Title { get; set; }
        public string? LinkUrl { get; set; }

        [Required]
        public string Position { get; set; } = null!; // "UpcomingTrips" or "PopularRoutes"

        public bool IsActive { get; set; } = true;

        // The uploaded image
        [Required]
        public IFormFile Image { get; set; } = null!;
    }
}
