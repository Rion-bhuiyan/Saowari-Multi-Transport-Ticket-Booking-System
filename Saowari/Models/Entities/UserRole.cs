using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.Entities
{
    public class UserRole
    {
        public int UserRoleId { get; set; }
        [Required,StringLength(30)]
        public string UserRoleName { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
