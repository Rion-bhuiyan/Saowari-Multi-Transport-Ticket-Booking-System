using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Saowari.Models.Entities
{
    /// <summary>
    /// Stores per-admin toggles so each admin can independently enable/disable
    /// company-specific ticket sale notifications.
    /// </summary>
    [Table("AdminNotificationPreference")]
    public class AdminNotificationPreference
    {
        [Key]
        public int Id { get; set; }

        /// <summary>The admin user this preference belongs to</summary>
        [Required]
        public int AdminUserId { get; set; }

        /// <summary>The company whose notifications can be toggled</summary>
        [Required]
        public int CompanyId { get; set; }

        /// <summary>When true (default), this admin receives company ticket notifications</summary>
        public bool IsEnabled { get; set; } = true;

        [ForeignKey("AdminUserId")]
        public virtual User? AdminUser { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
    }
}
