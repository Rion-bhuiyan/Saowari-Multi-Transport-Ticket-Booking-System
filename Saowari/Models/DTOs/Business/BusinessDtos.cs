using System;
using System.Collections.Generic;

namespace Saowari.Models.DTOs.Business
{
    public class TripSearchResultDto
    {
        public int ScheduleId { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = null!;
        public string VehicleNumber { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string FromLocation { get; set; } = null!;
        public string ToLocation { get; set; } = null!;
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public decimal BasePrice { get; set; }
        public int AvailableSeats { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public string? SeatLayoutConfig { get; set; }
        public List<string> SeatClassOptions { get; set; } = new List<string>();
        public DateTime? BoardingTime { get; set; }
    }

    public class FareSummaryDto
    {
        public decimal BaseAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        /// <summary>Net fare after discount, before fees</summary>
        public decimal NetAmount => BaseAmount - DiscountAmount;
        public decimal ProcessingFeeAmount { get; set; }
        public decimal VATAmount { get; set; }
        /// <summary>Grand total = NetAmount + ProcessingFeeAmount + VATAmount</summary>
        public decimal FinalAmount { get; set; }
        public string? DiscountName { get; set; }
        public string? PaymentMethodName { get; set; }
        public decimal ProcessingFeePercent { get; set; }
        public decimal VATPercent { get; set; }
    }

    public class RefundPreviewDto
    {
        public int BookingId { get; set; }
        public int PaymentId { get; set; }
        public double HoursUntilDeparture { get; set; }
        public string PolicyName { get; set; } = null!;
        public decimal RefundPercentage { get; set; }
        public decimal EligibleRefundAmount { get; set; }
        public decimal OriginalAmount { get; set; }
        public string Message { get; set; } = null!;
    }

    public class TicketVerificationDto
    {
        public bool IsValid { get; set; }
        public string TicketCode { get; set; } = null!;
        public string PassengerName { get; set; } = null!;
        public string SeatNumber { get; set; } = null!;
        public string Route { get; set; } = null!;
        public DateTime DepartureDateTime { get; set; }
        public string VehicleName { get; set; } = null!;
        public bool IsUsed { get; set; }
        public string Status { get; set; } = null!;
    }

    public class DashboardSummaryDto
    {
        public int TodayBookingsCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalActiveRoutes { get; set; }
        public int TotalActiveSchedules { get; set; }
        public int UpcomingDeparturesToday { get; set; }
        public List<dynamic> BookingsByStatus { get; set; } = new();
        public List<dynamic> RevenueByPaymentMethod { get; set; } = new();
    }

    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal NetRevenue { get; set; }
        public List<dynamic> BreakdownByPeriod { get; set; } = new();
        public List<dynamic> BreakdownByRoute { get; set; } = new();
    }

    public class OccupancyReportDto
    {
        public int ScheduleId { get; set; }
        public string RouteDisplay { get; set; } = null!;
        public DateTime DepartureDateTime { get; set; }
        public string VehicleName { get; set; } = null!;
        public int TotalSeats { get; set; }
        public int BookedSeats { get; set; }
        public double OccupancyPercent { get; set; }
    }

    public class InvoiceDto
    {
        public string InvoiceNumber { get; set; } = null!;
        public string BookingCode { get; set; } = null!;
        public string PassengerName { get; set; } = null!;
        public string Route { get; set; } = null!;
        public DateTime DepartureDateTime { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public string SeatClass { get; set; } = null!;
        public decimal BaseAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string TransactionID { get; set; } = null!;
        public DateTime PaidAt { get; set; }
        public List<string> TicketCodes { get; set; } = new();
        public DateTime IssuedAt { get; set; }
    }

    public class DiscountValidationDto
    {
        public bool IsValid { get; set; }
        public string? DiscountName { get; set; }
        public string? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal CalculatedDiscount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? InvalidReason { get; set; }
    }
}
