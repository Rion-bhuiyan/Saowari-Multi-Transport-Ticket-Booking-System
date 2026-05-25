namespace Saowari.Models.DTOs.RefundPolicy
{
    public class RefundPolicyUpdateDto
    {
        public int PolicyID { get; set; }
        public int CompanyId { get; set; }
        public string PolicyName { get; set; }
        public int HoursBeforeDeparture { get; set; }
        public decimal RefundPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}