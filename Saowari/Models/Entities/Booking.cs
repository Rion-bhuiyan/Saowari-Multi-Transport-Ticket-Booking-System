using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("Booking")]
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingID { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingCode { get; set; } = null!;

        [ForeignKey(nameof(User))]
        public int UserID { get; set; }

        [Required]
        [ForeignKey(nameof(Schedule))]
        public int ScheduleID { get; set; }

        [Required]
        [MaxLength(150)]
        public string PassengerName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Phone]
        public string PassengerPhone { get; set; } = null!;

        [MaxLength(30)]
        public string? PassengerNID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal BaseAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>Gateway/processing fee for the payment method chosen. Non-refundable.</summary>
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal ProcessingFeeAmount { get; set; } = 0;

        /// <summary>VAT on the processing fee. Non-refundable.</summary>
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal VATAmount { get; set; } = 0;

        /// <summary>Grand total = BaseAmount - DiscountAmount + ProcessingFeeAmount + VATAmount</summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal FinalAmount { get; set; }

        [ForeignKey(nameof(Discount))]
        public int? DiscountID { get; set; }

        [ForeignKey(nameof(BookingStatus))]
        public int? BookingStatusId { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        [MaxLength(150)]
        public string? BoardingPoint { get; set; }

        [MaxLength(10)]
        public string? CancellationOtp { get; set; }
        
        public DateTime? CancellationOtpExpiry { get; set; }

        [ForeignKey(nameof(SeatClass))]
        public int SeatClassId { get; set; }

        public virtual SeatClass? SeatClass { get; set; }
        public virtual Schedule? Schedule { get; set; }
        public virtual User? User { get; set; }
        public virtual BookingStatus? BookingStatus { get; set; }
        public virtual Discount? Discount { get; set; }
        public virtual ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public virtual ICollection<ScheduleSeatStatus> ScheduleSeatStatuses { get; set; } = new List<ScheduleSeatStatus>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}
