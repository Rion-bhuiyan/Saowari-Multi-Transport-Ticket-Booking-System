using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Auth
{
    public class UnlockAccountDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string OtpCode { get; set; } = null!;
    }
}
