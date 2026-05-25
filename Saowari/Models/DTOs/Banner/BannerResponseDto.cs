using System;

namespace Saowari.Models.DTOs.Banner
{
    public class BannerResponseDto
    {
        public int BannerId { get; set; }
        public string? Title { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? LinkUrl { get; set; }
        public string Position { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
