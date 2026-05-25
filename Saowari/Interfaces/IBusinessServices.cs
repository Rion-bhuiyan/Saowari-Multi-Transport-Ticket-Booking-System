using Saowari.Models.DTOs.Business;
using Saowari.Models.DTOs.Ticket;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ISearchService
    {
        Task<ApiResponse<IEnumerable<TripSearchResult>>> SearchTripsAsync(string transportType, int fromLocationId, int toLocationId, DateTime travelDate, int passengers, int? seatClassId);
        Task<ApiResponse<object>> GetSeatMapAsync(int scheduleId);
    }

    public interface IBookingFlowService
    {
        Task<ApiResponse<object>> ValidateSeatsAsync(int scheduleId, List<int> seatIds);
        Task<ApiResponse<FareSummaryDto>> GetFareSummaryAsync(int scheduleId, List<int> seatIds, int? discountId);
        Task<ApiResponse<object>> RescheduleAsync(int bookingId, int newScheduleId, List<int> newSeatIds);
    }

    public interface IRefundCalculationService
    {
        Task<ApiResponse<RefundPreviewDto>> CalculateRefundAsync(int bookingId);
        Task<ApiResponse<Models.DTOs.Refund.RefundResponseDto>> RequestRefundAsync(int bookingId, string remarks, int userId);
    }

    public interface ITicketBusinessService
    {
        Task<ApiResponse<IEnumerable<TicketResponseDto>>> IssueTicketsForBookingAsync(int bookingId);
        Task<ApiResponse<TicketVerificationDto>> VerifyTicketAsync(string ticketCode);
        Task<ApiResponse<object>> ScanTicketAsync(string ticketCode);
    }

    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync();
        Task<ApiResponse<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string groupBy);
        Task<ApiResponse<IEnumerable<OccupancyReportDto>>> GetOccupancyReportAsync(DateTime startDate, DateTime endDate);
    }

    public interface IUserProfileService
    {
        Task<ApiResponse<object>> GetMyBookingsAsync(int userId);
        Task<ApiResponse<InvoiceDto>> GetBookingInvoiceAsync(int userId, int bookingId);
    }

    public interface IDiscountValidationService
    {
        Task<ApiResponse<DiscountValidationDto>> ValidateDiscountAsync(int discountId, int scheduleId, decimal baseAmount);
    }

    // Workaround mapping for Search Result
    public class TripSearchResult : TripSearchResultDto {}
}
