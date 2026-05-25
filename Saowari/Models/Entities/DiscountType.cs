using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class DiscountType
    {
        public int DiscountTypeId { get; set; }
        [Required, MaxLength(50)]
        public string DiscountTypeName { get; set; } = null!;
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }
}
