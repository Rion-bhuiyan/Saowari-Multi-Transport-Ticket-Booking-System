using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    public class PaymentCancel
    {
        public int PaymentCancelId { get; set; }
        [StringLength(100)]
        public string VerificationCode { get; set; } = null!;
        [ForeignKey("Payment")]
        public int PaymentId { get; set; }

        public virtual Payment? Payment { get; set; }
    }
}
