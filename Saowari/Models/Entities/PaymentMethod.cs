using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("PaymentMethod")]
    public class PaymentMethod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentMethodId { get; set; }

        [Required, StringLength(50)]
        public string PaymentMethodName { get; set; } = null!;

        /// <summary>Percentage of the net fare charged as a processing/gateway fee. E.g. 1.5 = 1.5%</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal ProcessingFeePercent { get; set; } = 0;

        /// <summary>VAT applied on top of the processing fee only. E.g. 15 = 15%</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal VATPercent { get; set; } = 0;

        /// <summary>Optional logo URL stored in wwwroot/uploads/payment-methods/</summary>
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
