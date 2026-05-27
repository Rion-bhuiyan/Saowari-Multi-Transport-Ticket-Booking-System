namespace Saowari.Models.DTOs.User
{
    public class RequestEmailChangeDto
    {
        public string NewEmail { get; set; } = null!;
    }

    public class VerifyEmailChangeStep1Dto
    {
        public string CurrentEmailOtp { get; set; } = null!;
    }

    public class VerifyEmailChangeStep2Dto
    {
        public string NewEmailOtp { get; set; } = null!;
    }
}
