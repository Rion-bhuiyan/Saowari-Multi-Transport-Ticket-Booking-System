using Microsoft.AspNetCore.Http;

namespace Saowari.Models.DTOs.Company
{
    public class CompanyCreateDto
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public int CompanyTypeId { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? LogoURL { get; set; }
        public IFormFile? LogoFile { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}