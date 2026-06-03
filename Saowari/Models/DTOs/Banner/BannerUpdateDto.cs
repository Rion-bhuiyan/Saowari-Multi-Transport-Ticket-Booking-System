using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Banner
{
    public class BannerUpdateDto
    {
        public string? Title { get; set; }
        public string? LinkUrl { get; set; }

        [Required]
        public string Position { get; set; } = null!;

        [Required]
        public string SizeTemplate { get; set; } = "Horizontal";

        public bool IsActive { get; set; }

        // Optional new image
        public IFormFile? Image { get; set; }
    }
}
