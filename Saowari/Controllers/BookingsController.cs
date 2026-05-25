using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Booking;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingsController(IBookingService service)
        {
            _service = service;
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
            var bookingCode = slug.Split('-')[0];
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
            return CreatedAtAction(nameof(GetById), new { id = 0 }, result);
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
    }
}