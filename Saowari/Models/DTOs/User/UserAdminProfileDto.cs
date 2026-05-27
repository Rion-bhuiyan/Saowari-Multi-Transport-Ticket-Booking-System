using System;
using System.Collections.Generic;

namespace Saowari.Models.DTOs.User
{
    public class UserAdminProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
        public string? RoleName { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminCopyEmail { get; set; }
        
        public List<AdminLoginHistoryDto> LoginHistory { get; set; } = new();
        public List<AdminUserBookingDto> Bookings { get; set; } = new();
    }

    public class AdminLoginHistoryDto
    {
        public string IpAddress { get; set; } = null!;
        public string DeviceName { get; set; } = null!;
        public DateTime LoginTime { get; set; }
        
        // These fields are simulated/resolved on frontend or backend for now
        public string? Location { get; set; } 
        public string? Isp { get; set; }
        public string? Country { get; set; }
    }

    public class AdminUserBookingDto
    {
        public int BookingID { get; set; }
        public string BookingCode { get; set; } = null!;
        public DateTime BookingDate { get; set; }
        public decimal FinalAmount { get; set; }
        public string? BookingStatus { get; set; }
        
        public string? PassengerName { get; set; }
        public string? BoardingPoint { get; set; }

        public int ScheduleID { get; set; }
        public string? RouteName { get; set; }
        
        public int? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehiclePlateNumber { get; set; }
        
        public string? CompanyName { get; set; }
    }
}
