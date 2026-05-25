namespace Saowari.Models.ViewModels
{
    public class UserViewModel
    {
        // Identity
        public int UserID { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Picture { get; set; }
        public IFormFile? PictureFile { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Role & Relations
        public int RoleID { get; set; }
        public string? RoleName { get; set; }         // from UserRole nav prop

        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }       // from Company nav prop

        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }    // from Supervisor nav prop

        public int? DriverInformtionId { get; set; }
    }
}