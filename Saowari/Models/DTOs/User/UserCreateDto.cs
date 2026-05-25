namespace Saowari.Models.DTOs.User
{
    public class UserCreateDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Picture { get; set; }
        public int RoleID { get; set; }
        public int? DriverInformtionId { get; set; }
        public int? SupervisorId { get; set; }
        public int? CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}