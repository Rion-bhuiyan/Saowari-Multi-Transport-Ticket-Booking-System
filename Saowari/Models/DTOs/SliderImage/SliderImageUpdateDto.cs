using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.SliderImage
{
    public class SliderImageUpdateDto
    {
        public int SliderImageID { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? Subtitle { get; set; }

        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }
    }
}
