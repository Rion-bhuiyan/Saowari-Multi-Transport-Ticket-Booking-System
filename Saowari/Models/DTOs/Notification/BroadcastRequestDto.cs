using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Saowari.Models.DTOs.Notification
{
    public class BroadcastRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Subject { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        public List<int>? TargetRoleIds { get; set; }

        public IFormFile? Image { get; set; }
    }
}
