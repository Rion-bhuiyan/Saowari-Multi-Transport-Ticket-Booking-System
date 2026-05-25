using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;

namespace Saowari.Models.Entities
{
    [Table("RefundPolicy")]
    public class RefundPolicy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PolicyID { get; set; }

        [Required]
        [ForeignKey(nameof(Company))]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PolicyName { get; set; } = null!;

        [Required]
        [Range(0, 8760)] 
        public int HoursBeforeDeparture { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal RefundPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        
        public virtual Company? Company { get; set; }
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
