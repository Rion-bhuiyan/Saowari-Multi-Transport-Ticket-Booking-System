using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("Company")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyID { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = null!;

        [Required]
        [ForeignKey("CompanyType")]
        public int CompanyTypeId { get; set; }

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string ContactEmail { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Phone]
        public string ContactPhone { get; set; } = null!;

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(500)]
        [Url]
        public string? LogoURL { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual CompanyType? CompanyType { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        public virtual ICollection<RefundPolicy> RefundPolicies { get; set; } = new List<RefundPolicy>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
