using System.ComponentModel.DataAnnotations;
using Saowari.Models.DTOs.User;

namespace Saowari.Models.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        public string Phone { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public int RoleID { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserResponseDto User { get; set; } = null!;
    }

    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = null!;
    }

    public class VerifyRegistrationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string OtpCode { get; set; } = null!;
    }
}
