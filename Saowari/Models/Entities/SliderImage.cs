using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("SliderImage")]
    public class SliderImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SliderImageID { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;

        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? Subtitle { get; set; }

        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
