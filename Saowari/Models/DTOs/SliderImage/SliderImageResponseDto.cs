namespace Saowari.Models.DTOs.SliderImage
{
    public class SliderImageResponseDto
    {
        public int SliderImageID { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? LinkUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
