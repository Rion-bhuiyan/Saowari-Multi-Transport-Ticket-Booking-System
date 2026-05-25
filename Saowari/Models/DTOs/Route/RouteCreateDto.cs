using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Route
{
    public class RouteCreateDto
    {
        [Required]
        public int FromLocationID { get; set; }

        [Required]
        public int ToLocationID { get; set; }

        public decimal? DistanceKM { get; set; }
        public decimal? EstimatedHours { get; set; }
        public bool IsActive { get; set; } = true;
        
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }    }
}