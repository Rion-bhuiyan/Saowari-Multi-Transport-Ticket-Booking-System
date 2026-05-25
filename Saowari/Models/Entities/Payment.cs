using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentID { get; set; }

        [Required]
        [ForeignKey(nameof(Booking))]
        public int BookingId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal ProcessingFeeAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal VATAmount { get; set; } = 0;

        [Required]
        [ForeignKey("PaymentMethod")]
        public int PaymentMethodId { get; set; }

        [MaxLength(100)]
        public string? TransactionID { get; set; }

        [Required]
        [ForeignKey("PaymentStatus")]
        public int PaymentStatusId { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      
        
        public virtual Booking? Booking { get; set; }

        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual PaymentStatus? PaymentStatus { get; set; }
        public virtual ICollection<PaymentCancel> PaymentCancels { get; set; } = new List<PaymentCancel>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
