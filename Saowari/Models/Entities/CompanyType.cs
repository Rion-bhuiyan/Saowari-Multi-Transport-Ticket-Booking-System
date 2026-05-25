using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class CompanyType
    {
        public int CompanyTypeId { get; set; }
        [Required, MaxLength(100)]
        public string CompanyTypeName { get; set; } = null!;
        public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}
