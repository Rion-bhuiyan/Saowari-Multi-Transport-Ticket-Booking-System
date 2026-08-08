using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Auth
{
    public class ResendOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        public string Type { get; set; } = null!; // e.g. "unlock", "login"
    }
}
