using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Saowari.Models.Entities
{
    [Table("Refund")]
    public class Refund
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RefundID { get; set; }

        [Required]
        [ForeignKey(nameof(Booking))]
        public int BookingId { get; set; }


        [Required]
        [ForeignKey(nameof(Payment))]
        public int PaymentId { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal RefundPercentage { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal RefundAmount { get; set; }

        [Required]
        [ForeignKey("RefundStatus")]
        public int RefundStatusId { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(100)]
        public string? RefundTransactionID { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public bool IsRefunded { get; set; }

        [MaxLength(6)]
        public string? RefundOtpCode { get; set; }
        public DateTime? RefundOtpExpireTime { get; set; }

        [ForeignKey(nameof(RefundPolicy))]
        public int PolicyID { get; set; }

        // Audit fields — who updated the status and when
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual RefundStatus? RefundStatus { get; set; }
        public virtual Payment? Payment { get; set; }
        public virtual Booking? Booking { get; set; }
        public virtual RefundPolicy? RefundPolicy { get; set; }
        [ForeignKey(nameof(UpdatedByUserId))]
        public virtual User? UpdatedByUser { get; set; }
    }
}
