using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class RefundStatus
    {
        public int RefundStatusId { get; set; }
        [Required,StringLength(20)]
        public string StatusName { get; set; } = null!;
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
