using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Booking;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Saowari.Data;
using Microsoft.EntityFrameworkCore;
using Saowari.Services;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;
        private readonly IEmailService _emailService;
        private readonly SaowariDbContext _context;

        public BookingsController(IBookingService service, IEmailService emailService, SaowariDbContext context)
        {
            _service = service;
            _emailService = emailService;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponseDto>>>> GetMy()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var result = await _service.GetMyAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}/ticket")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TicketDetailsDto>>> GetTicket(int id)
        {
            var result = await _service.GetTicketDetailsAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("code/{slug}/ticket")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TicketDetailsDto>>> GetTicketByCode(string slug)
        {
            // Slug format: BookingCode-CustomerName (e.g. B260519115835144-NakibulRaju)
            // Booking codes start with B followed by digits, so we extract up to the first '-' after the code
            var bookingCode = slug.Split('-')[0].Trim();
            var result = await _service.GetTicketDetailsByCodeAsync(bookingCode);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> Create([FromBody] BookingCreateDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                dto.UserID = userId;
            }

            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            
            // Send Ticket Email
            if (result.Data != null && result.Data.UserID > 0)
            {
                var user = await _context.Users.FindAsync(result.Data.UserID);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    // Fetch full schedule details to get From/To locations properly
                    var schedule = await _context.Schedules
                        .Include(s => s.Route)
                            .ThenInclude(r => r.FromLocation)
                        .Include(s => s.Route)
                            .ThenInclude(r => r.ToLocation)
                        .FirstOrDefaultAsync(s => s.ScheduleID == dto.ScheduleID);

                    var fromLocation = schedule?.Route?.FromLocation?.LocationName ?? result.Data.FromLocation ?? "Unknown";
                    var toLocation = schedule?.Route?.ToLocation?.LocationName ?? result.Data.ToLocation ?? "Unknown";
                    var departureTime = schedule?.DepartureDateTime.ToString("f") ?? result.Data.DepartureDateTime?.ToString("f") ?? "N/A";

                    var ticketLink = $"http://localhost:4200/ticket/{result.Data.BookingCode}-{result.Data.PassengerName}";
                    
                    var htmlBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2>Ticket Confirmed!</h2>
                            <p>Hello {user.FullName},</p>
                            <p>Your booking <b>#{result.Data.BookingCode}</b> was successful!</p>
                            <ul>
                                <li><b>Passenger:</b> {result.Data.PassengerName}</li>
                                <li><b>Amount Paid:</b> ৳{result.Data.FinalAmount:N0}</li>
                                <li><b>Departure:</b> {departureTime}</li>
                                <li><b>From:</b> {fromLocation} <b>To:</b> {toLocation}</li>
                            </ul>
                            <div style='margin-top: 20px;'>
                                <a href='{ticketLink}' style='padding: 12px 24px; background-color: #0284c7; color: #fff; text-decoration: none; border-radius: 6px;'>View / Download Ticket</a>
                            </div>
                        </div>";
                        
                    var plainBody = $"Your booking {result.Data.BookingCode} was successful. Download ticket at {ticketLink}";

                    try 
                    {
                        await _emailService.SendEmailAsync(user.Email, $"Your Saowari Ticket - {result.Data.BookingCode}", htmlBody, plainBody);
                    } catch { /* fail safe */ }
                }
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.BookingID ?? 0 }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<BookingResponseDto>>> Update(int id, [FromBody] BookingUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{id}/request-cancel")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> RequestCancel(int id)
        {
            // Verify ownership — customer can only cancel their own booking
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound(ApiResponse<bool>.Fail("Booking not found."));
            
            // Non-admin users can only cancel their own bookings
            if (userRole != "Admin" && userRole != "CompanyManager")
            {
                if (!int.TryParse(userIdStr, out int userId) || booking.UserID != userId)
                    return Forbid();
            }

            var result = await _service.RequestCancellationAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{id}/verify-cancel")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyCancel(int id, [FromBody] VerifyCancelDto dto)
        {
            if (string.IsNullOrEmpty(dto.Otp)) return BadRequest(ApiResponse<bool>.Fail("OTP is required"));
            
            // Verify ownership
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound(ApiResponse<bool>.Fail("Booking not found."));
            
            if (userRole != "Admin" && userRole != "CompanyManager")
            {
                if (!int.TryParse(userIdStr, out int userId) || booking.UserID != userId)
                    return Forbid();
            }

            var result = await _service.VerifyCancellationAsync(id, dto.Otp);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }

    public class VerifyCancelDto
    {
        public string Otp { get; set; } = string.Empty;
    }
}