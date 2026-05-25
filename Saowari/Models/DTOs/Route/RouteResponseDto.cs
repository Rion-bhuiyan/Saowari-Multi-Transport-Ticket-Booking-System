namespace Saowari.Models.DTOs.Route
{
    public class RouteResponseDto
    {
        public int RouteID { get; set; }
        public int FromLocationID { get; set; }
        public int ToLocationID { get; set; }
        public decimal? DistanceKM { get; set; }
        public decimal? EstimatedHours { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }    }
}