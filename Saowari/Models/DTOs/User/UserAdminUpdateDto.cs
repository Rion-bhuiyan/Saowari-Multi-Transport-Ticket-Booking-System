using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.User
{
    public class UserAdminUpdateDto
    {
        [Required]
        public string FullName { get; set; } = null!;
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Phone { get; set; } = null!;
        public string? Picture { get; set; }
        public string? AdminCopyEmail { get; set; }
        
        [Required]
        public int RoleID { get; set; }
        
        // Admin Dynamic Fields
        public int? CompanyId { get; set; }
        public string? LicenceNumber { get; set; }
        public DateTime? LicenceExpDate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
