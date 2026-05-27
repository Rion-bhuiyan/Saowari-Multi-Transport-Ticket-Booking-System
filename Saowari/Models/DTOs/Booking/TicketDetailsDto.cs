using System;
using System.Collections.Generic;

namespace Saowari.Models.DTOs.Booking
{
    public class SeatDetailDto
    {
        public string SeatNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class TicketDetailsDto
    {
        public int BookingID { get; set; }
        public string? BookingCode { get; set; }
        public string? PassengerName { get; set; }
        public string? PassengerPhone { get; set; }
        public string? PassengerNID { get; set; }
        public DateTime BookingDate { get; set; }

        public string SaowariLogoUrl { get; set; } = string.Empty;
        public string? TicketBackgroundUrl { get; set; }
        public decimal TicketBackgroundOpacity { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyLogoUrl { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleRegNumber { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public bool IsAc { get; set; }

        public string SupervisorName { get; set; } = string.Empty;
        public string SupervisorPhone { get; set; } = string.Empty;

        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public string? BoardingPoint { get; set; }
        public string RouteImageUrl { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public decimal BaseAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ProcessingFeeAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal FinalAmount { get; set; }

        /// <summary>Seat unit price (BaseAmount / SeatCount)</summary>
        public decimal PricePerSeat { get; set; }
        public int SeatCount { get; set; }

        /// <summary>Coupon/discount code applied, if any</summary>
        public string? CouponCode { get; set; }
        public string? DiscountName { get; set; }
        public bool IsPercentageDiscount { get; set; }
        public decimal DiscountValue { get; set; }
        
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }

        public List<string> SeatNumbers { get; set; } = new List<string>();
        public List<SeatDetailDto> SeatDetails { get; set; } = new List<SeatDetailDto>();
        public string Status { get; set; } = string.Empty;
    }
}
