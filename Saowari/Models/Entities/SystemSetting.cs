using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("SystemSetting")]
    public class SystemSetting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; } = null!;

        public string? Value { get; set; }
    }
}
