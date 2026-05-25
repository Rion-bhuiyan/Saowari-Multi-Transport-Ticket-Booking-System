using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Business;
using Saowari.Models.DTOs.Ticket;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    // ── Search ────────────────────────────────────────────────────────────────
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService) => _searchService = searchService;

        /// <summary>Search available trips</summary>
        [HttpGet("trips")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripSearchResultDto>>>> SearchTrips(
            [FromQuery] string transportType,
            [FromQuery] int fromLocationId,
            [FromQuery] int toLocationId,
            [FromQuery] DateTime travelDate,
            [FromQuery] int passengers,
            [FromQuery] int? seatClass)
        {
            var result = await _searchService.SearchTripsAsync(transportType, fromLocationId, toLocationId, travelDate, passengers, seatClass);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Get seat map for a schedule</summary>
        [HttpGet("seat-map/{scheduleId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> GetSeatMap(int scheduleId)
        {
            var result = await _searchService.GetSeatMapAsync(scheduleId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    // ── Booking Flow ──────────────────────────────────────────────────────────
    [Route("api/bookings/flow")]
    [ApiController]
    public class BookingFlowController : ControllerBase
    {
        private readonly IBookingFlowService _bookingService;

        public BookingFlowController(IBookingFlowService bookingService) => _bookingService = bookingService;

        /// <summary>Validate seat availability before booking</summary>
        [HttpPost("validate-seats")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ValidateSeats([FromBody] ValidateSeatsRequest req)
        {
            var result = await _bookingService.ValidateSeatsAsync(req.ScheduleId, req.SeatIds);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Get fare summary with optional discount</summary>
        [HttpGet("fare-summary")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FareSummaryDto>>> GetFareSummary(
            [FromQuery] int scheduleId, [FromQuery] string seatIds, [FromQuery] int? discountId)
        {
            var ids = new List<int>();
            if (!string.IsNullOrEmpty(seatIds))
                foreach (var s in seatIds.Split(','))
                    if (int.TryParse(s, out int i)) ids.Add(i);

            var result = await _bookingService.GetFareSummaryAsync(scheduleId, ids, discountId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    public class ValidateSeatsRequest
    {
        public int ScheduleId { get; set; }
        public List<int> SeatIds { get; set; } = new();
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

        /// <summary>Get admin dashboard summary</summary>
        [HttpGet("summary")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary()
        {
            var result = await _dashboardService.GetSummaryAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    // ── Ticket Business ───────────────────────────────────────────────────────
    [Route("api/tickets")]
    [ApiController]
    public class TicketBusinessController : ControllerBase
    {
        private readonly ITicketBusinessService _ticketService;

        public TicketBusinessController(ITicketBusinessService ticketService) => _ticketService = ticketService;

        /// <summary>Issue tickets for a confirmed booking</summary>
        [HttpPost("issue-for-booking/{bookingId}")]
        [Authorize(Policy = "AnyStaff")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TicketResponseDto>>>> IssueTickets(int bookingId)
        {
            var result = await _ticketService.IssueTicketsForBookingAsync(bookingId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Verify a ticket by its code (QR scan)</summary>
        [HttpGet("verify/{ticketCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TicketVerificationDto>>> VerifyTicket(string ticketCode)
        {
            var result = await _ticketService.VerifyTicketAsync(ticketCode);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Mark a ticket as used</summary>
        [HttpPatch("scan/{ticketCode}")]
        [Authorize(Policy = "AnyStaff")]
        public async Task<ActionResult<ApiResponse<object>>> ScanTicket(string ticketCode)
        {
            var result = await _ticketService.ScanTicketAsync(ticketCode);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    // ── Refund Calculation ────────────────────────────────────────────────────
    [Route("api/refunds")]
    [ApiController]
    public class RefundCalculationController : ControllerBase
    {
        private readonly IRefundCalculationService _refundService;

        public RefundCalculationController(IRefundCalculationService refundService) => _refundService = refundService;

        /// <summary>Preview refund amount for a booking</summary>
        [HttpGet("calculate")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RefundPreviewDto>>> CalculateRefund([FromQuery] int bookingId)
        {
            var result = await _refundService.CalculateRefundAsync(bookingId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Submit a refund request for a booking</summary>
        [HttpPost("request")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Models.DTOs.Refund.RefundResponseDto>>> RequestRefund([FromBody] SubmitRefundRequestDto req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var result = await _refundService.RequestRefundAsync(req.BookingId, req.Remarks ?? "", userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    public class SubmitRefundRequestDto
    {
        public int BookingId { get; set; }
        public string? Remarks { get; set; }
    }

    // ── User Profile ──────────────────────────────────────────────────────────
    [Route("api/users/me")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userService;

        public UserProfileController(IUserProfileService userService) => _userService = userService;

        /// <summary>Get current user's bookings</summary>
        [HttpGet("bookings")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> GetMyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var result = await _userService.GetMyBookingsAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Get invoice for a specific booking</summary>
        [HttpGet("bookings/{bookingId}/invoice")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoice(int bookingId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var result = await _userService.GetBookingInvoiceAsync(userId, bookingId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
