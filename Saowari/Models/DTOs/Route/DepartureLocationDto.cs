using System;

namespace Saowari.Models.DTOs.Route
{
    public class DepartureLocationDto
    {
        public int LocationID { get; set; }
        public string? LocationName { get; set; }
        public TimeSpan Time { get; set; }
        public decimal? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
