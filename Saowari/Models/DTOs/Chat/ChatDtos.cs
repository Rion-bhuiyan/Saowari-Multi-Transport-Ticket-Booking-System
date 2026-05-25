using System;

namespace Saowari.Models.DTOs.Chat
{
    public class SupportRoomDto
    {
        public int Id { get; set; }
        public string UserEmailOrIP { get; set; } = null!;
        public int? AssignedAdminId { get; set; }
        public string? AssignedAdminName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessageContent { get; set; }
    }

    public class SupportMessageDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string SenderName { get; set; } = null!;
        public int? SenderId { get; set; }
        public string Content { get; set; } = null!;
        public string MessageType { get; set; } = "text";
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class ScheduleChatMessageDto
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string MessageType { get; set; } = "text";
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = null!;
        public string MessageType { get; set; } = "text";
        public string? FileUrl { get; set; }
    }
}
