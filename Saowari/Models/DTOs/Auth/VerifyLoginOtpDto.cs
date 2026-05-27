using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Auth
{
    public class VerifyLoginOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = null!;
    }
}
