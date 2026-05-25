using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class PaymentStatus
    {
        public int PaymentStatusId { get; set; }
        [Required, MaxLength(100)]
        public string PaymentStatusName { get; set; }=null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
