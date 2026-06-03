using System;

namespace Saowari.Models.DTOs.Chat
{
    public class ScheduleChatMemberDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsRemoved { get; set; }
        public DateTime? RemovedAt { get; set; }
    }
}
