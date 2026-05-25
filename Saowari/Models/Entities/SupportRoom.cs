using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    [Table("SupportRoom")]
    public class SupportRoom
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserEmailOrIP { get; set; } = null!;

        [ForeignKey("AssignedAdmin")]
        public int? AssignedAdminId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public virtual User? AssignedAdmin { get; set; }
        public virtual ICollection<SupportMessage> SupportMessages { get; set; } = new List<SupportMessage>();
    }
}
