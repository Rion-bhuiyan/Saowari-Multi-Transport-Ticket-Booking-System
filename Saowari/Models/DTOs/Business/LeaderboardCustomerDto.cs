using System;

namespace Saowari.Models.DTOs.Business
{
    public class LeaderboardCustomerDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Picture { get; set; }
        
        public int TotalTickets { get; set; }
        public decimal TotalSpent { get; set; }
        
        // These are global for the user (not company specific)
        public int TotalLogins { get; set; }
        public double TotalTimeSpentMinutes { get; set; }
    }
}
