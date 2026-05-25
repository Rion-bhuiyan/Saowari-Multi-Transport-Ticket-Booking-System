using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;

namespace Saowari.Models.Entities
{
    [Table("Discount")]
    public class Discount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DiscountID { get; set; }

        [Required]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        [ForeignKey(nameof(Route))]
        public int? RouteId { get; set; }

        [ForeignKey(nameof(VehicleType))]
        public int? VehicleTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DiscountName { get; set; } = null!;

        [MaxLength(50)]
        public string? CouponCode { get; set; }

        [Required]
        [ForeignKey("DiscountType")]
        public int DiscountTypeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal? MinTicketAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        

        
        public virtual VehicleType? VehicleType { get; set; }
        public virtual DiscountType? DiscountType { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Route? Route { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
