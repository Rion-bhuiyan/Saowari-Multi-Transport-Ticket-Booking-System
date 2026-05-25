namespace Saowari.Models.DTOs.Location
{
    public class LocationResponseDto
    {
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public int LocationCode { get; set; }
        public decimal? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? District { get; set; }
        public bool IsActive { get; set; }
    }
}