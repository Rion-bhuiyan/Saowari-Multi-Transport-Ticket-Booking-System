namespace Saowari.Models.DTOs.User
{
    /// <summary>Safe response DTO — never includes PasswordHash or RefreshToken</summary>
    public class UserResponseDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Picture { get; set; }
        public string? AdminCopyEmail { get; set; }
        public int RoleID { get; set; }
        public string? RoleName { get; set; }
        public int? DriverInformtionId { get; set; }
        public int? SupervisorId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}