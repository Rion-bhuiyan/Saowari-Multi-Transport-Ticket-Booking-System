using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Ticket;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _service;

        public TicketsController(ITicketService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<TicketResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TicketResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<TicketResponseDto>>> Create([FromBody] TicketCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = 0 }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<TicketResponseDto>>> Update(int id, [FromBody] TicketUpdateDto dto)
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

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<TicketResponseDto>>>> GetMyTickets()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var result = await _service.GetMyTicketsAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-booking/{bookingId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<TicketResponseDto>>>> GetByBooking(int bookingId)
        {
            var result = await _service.GetByBookingAsync(bookingId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-code/{code}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TicketResponseDto>>> GetByCode(string code)
        {
            var result = await _service.GetByCodeAsync(code);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}