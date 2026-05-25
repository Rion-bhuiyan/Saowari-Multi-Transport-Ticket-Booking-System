using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Refund;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefundsController : ControllerBase
    {
        private readonly IRefundService _service;
        private readonly INotificationService _notificationService;
        private readonly SaowariDbContext _context;

        public RefundsController(IRefundService service, INotificationService notificationService, SaowariDbContext context)
        {
            _service = service;
            _notificationService = notificationService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RefundResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.Success) return BadRequest(result);

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    // Filter refunds to only those belonging to the manager's company
                    var validBookingIds = await _context.Bookings
                        .Include(b => b.Schedule.Vehicle)
                        .Where(b => b.Schedule.Vehicle.CompanyId == companyId)
                        .Select(b => b.BookingID)
                        .ToListAsync();

                    result.Data = result.Data.Where(r => validBookingIds.Contains(r.BookingId)).ToList();
                }
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RefundResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<RefundResponseDto>>> Create([FromBody] RefundCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);

            if (result.Data != null)
            {
                try
                {
                    var refund = new Refund 
                    { 
                        RefundID = result.Data.RefundID, 
                        BookingId = result.Data.BookingId, 
                        RefundAmount = result.Data.RefundAmount,
                        RefundStatusId = result.Data.RefundStatusId
                    };
                    await _notificationService.NotifyRefundRequestedAsync(refund);
                }
                catch (System.Exception) { /* Fail-safe */ }
            }

            return CreatedAtAction(nameof(GetById), new { id = 0 }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<RefundResponseDto>>> Update(int id, [FromBody] RefundUpdateDto dto)
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

        [HttpPatch("{id}/status")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> PatchStatus(int id, [FromBody] RefundStatusPatchDto dto)
        {
            var refund = await _service.GetByIdAsync(id);
            if (!refund.Success || refund.Data == null) 
                return NotFound(ApiResponse<bool>.Fail("Refund not found."));

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var booking = await _context.Bookings
                        .Include(b => b.Schedule.Vehicle)
                        .FirstOrDefaultAsync(b => b.BookingID == refund.Data.BookingId);
                    
                    if (booking != null && booking.Schedule.Vehicle.CompanyId != companyId)
                    {
                        return Forbid("You can only manage refunds for your own company.");
                    }
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? updatedByUserId = null;
            if (int.TryParse(userIdStr, out int uid))
            {
                updatedByUserId = uid;
            }

            var updateDto = new RefundUpdateDto
            {
                RefundID = refund.Data.RefundID,
                BookingId = refund.Data.BookingId,
                PaymentId = refund.Data.PaymentId,
                RequestedAt = refund.Data.RequestedAt,
                RefundPercentage = refund.Data.RefundPercentage,
                RefundAmount = refund.Data.RefundAmount,
                RefundStatusId = dto.StatusId,
                ProcessedAt = (dto.StatusId == 2 || dto.StatusId == 4) ? System.DateTime.UtcNow : refund.Data.ProcessedAt,
                RefundTransactionID = refund.Data.RefundTransactionID,
                Remarks = refund.Data.Remarks,
                IsRefunded = (dto.StatusId == 2 || dto.StatusId == 4),
                PolicyID = refund.Data.PolicyID,
                UpdatedByUserId = updatedByUserId,
                UpdatedAt = System.DateTime.UtcNow
            };

            var updateResult = await _service.UpdateAsync(id, updateDto);
            if (!updateResult.Success) 
                return BadRequest(ApiResponse<bool>.Fail("Failed to update status."));

            try
            {
                var updatedRefund = new Refund 
                { 
                    RefundID = refund.Data.RefundID, 
                    BookingId = refund.Data.BookingId, 
                    RefundAmount = refund.Data.RefundAmount,
                    RefundStatusId = dto.StatusId
                };
                await _notificationService.NotifyRefundProcessedAsync(updatedRefund);
            }
            catch (System.Exception) { /* Fail-safe */ }

            return Ok(ApiResponse<bool>.Ok(true, "Refund status updated successfully."));
        }
    }
}