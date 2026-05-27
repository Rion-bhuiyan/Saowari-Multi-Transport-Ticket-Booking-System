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
        private readonly Saowari.Services.IEmailService _emailService;

        public RefundsController(IRefundService service, INotificationService notificationService, SaowariDbContext context, Saowari.Services.IEmailService emailService)
        {
            _service = service;
            _notificationService = notificationService;
            _context = context;
            _emailService = emailService;
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

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<IEnumerable<RefundResponseDto>>>> GetMy()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var refunds = await _context.Refunds
                .Include(r => r.Booking)
                .Include(r => r.RefundStatus)
                .Where(r => r.Booking.UserID == userId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new RefundResponseDto
                {
                    RefundID = r.RefundID,
                    BookingId = r.BookingId,
                    PaymentId = r.PaymentId,
                    RequestedAt = r.RequestedAt,
                    RefundPercentage = r.RefundPercentage,
                    RefundAmount = r.RefundAmount,
                    RefundStatusId = r.RefundStatusId,
                    ProcessedAt = r.ProcessedAt,
                    RefundTransactionID = r.RefundTransactionID,
                    Remarks = r.Remarks,
                    IsRefunded = r.IsRefunded,
                    PolicyID = r.PolicyID,
                    BookingCode = r.Booking.BookingCode,
                    RefundStatusName = r.RefundStatus != null ? r.RefundStatus.StatusName : null,
                    // Additional check for OTP state
                    RequiresOtp = (r.RefundStatusId == 2 && r.RefundOtpCode != null && !r.IsRefunded)
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<RefundResponseDto>>.Ok(refunds));
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
                // Only mark as processed/completed when status is 4 (Completed) - NOT when just approved (2)
                // Approved (2) means OTP was sent, actual refund happens only on OTP verify
                ProcessedAt = dto.StatusId == 4 ? System.DateTime.UtcNow : refund.Data.ProcessedAt,
                RefundTransactionID = refund.Data.RefundTransactionID,
                Remarks = refund.Data.Remarks,
                // IsRefunded = true only when Completed (4), not when just Approved (2)
                IsRefunded = dto.StatusId == 4,
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
                
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingID == refund.Data.BookingId);

                // Send email to passenger if approved (status 2 = Approved, OTP required)
                if (dto.StatusId == 2) // Approved - generate OTP and send to customer
                {
                    if (booking != null && booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
                    {
                        var otp = new System.Random().Next(100000, 999999).ToString();
                        
                        // Store OTP on the refund entity; IsRefunded remains FALSE until OTP is verified
                        var refundEntity = await _context.Refunds.FindAsync(id);
                        if (refundEntity != null)
                        {
                            refundEntity.RefundOtpCode = otp;
                            refundEntity.RefundOtpExpireTime = System.DateTime.UtcNow.AddMinutes(30);
                            refundEntity.IsRefunded = false; // MUST stay false until customer verifies OTP
                            await _context.SaveChangesAsync();
                        }

                        Console.WriteLine($"\n=======================================================");
                        Console.WriteLine($"REFUND OTP GENERATED");
                        Console.WriteLine($"Refund ID: {id}, Booking: {booking.BookingCode}");
                        Console.WriteLine($"Customer Email: {booking.User.Email}");
                        Console.WriteLine($"OTP Code: {otp}");
                        Console.WriteLine($"=======================================================\n");

                        // Push real-time notification to customer
                        try
                        {
                            await _notificationService.CreateForUserAsync(
                                booking.UserID,
                                "Refund Approved - OTP Required",
                                $"Your refund of ৳{refund.Data.RefundAmount:N0} for booking #{booking.BookingCode} is approved! Your OTP is: {otp}. Visit 'My Refunds' to verify and complete the refund.",
                                "refund", "fas fa-check-circle", "bg-green-100 text-green-600",
                                "Refund", id);
                        }
                        catch (System.Exception ex) { Console.WriteLine($"SignalR notification failed: {ex.Message}"); }

                        var htmlBody = $@"
<div style=""font-family: 'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0b0f19; color: #e2e8f0; padding: 40px 20px; text-align: center;"">
    <div style=""max-width: 500px; margin: 0 auto; background: linear-gradient(145deg, #111827, #1f2937); padding: 40px; border-radius: 20px; box-shadow: 0 10px 30px rgba(0,0,0,0.5); border: 1px solid #374151;"">
        <h2 style=""color: #10b981; font-size: 24px; margin-bottom: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 2px;"">Refund Approved</h2>
        <p style=""color: #9ca3af; font-size: 16px; margin-bottom: 10px;"">Hello <strong style=""color:#e2e8f0;"">{booking.User.FullName}</strong>,</p>
        <p style=""color: #9ca3af; font-size: 16px; margin-bottom: 30px; line-height: 1.5;"">The refund for your booking <b>#{booking.BookingCode}</b> has been approved. To complete the transfer of <strong style=""color:#a7f3d0;"">৳{refund.Data.RefundAmount:N0}</strong>, use the secure OTP below.</p>
        <div style=""background-color: #064e3b; padding: 20px; border-radius: 12px; border: 1px solid #059669; display: inline-block; margin-bottom: 30px; box-shadow: 0 0 20px rgba(16, 185, 129, 0.2);"">
            <span style=""font-size: 36px; font-weight: bold; color: #a7f3d0; letter-spacing: 12px; font-family: monospace;"">{otp}</span>
        </div>
        <p style=""color: #6b7280; font-size: 14px; margin-top: 20px;"">This code will expire in 30 minutes.<br>Enter it in your 'My Refunds' page to finalize the refund.</p>
        <div style=""margin-top: 40px; padding-top: 20px; border-top: 1px solid #374151;"">
            <span style=""color: #10b981; font-weight: bold; font-size: 18px;"">Saowari</span><br>
            <span style=""color: #6b7280; font-size: 12px;"">Next-Generation Ticketing</span>
        </div>
    </div>
</div>";
                            
                        var plainBody = $"Your refund of ৳{refund.Data.RefundAmount:N0} for booking {booking.BookingCode} is approved. Your confirmation OTP is {otp}. Expires in 30 minutes. Go to My Refunds to verify.";

                        try { await _emailService.SendEmailAsync(booking.User.Email, "Saowari Refund Approved - OTP Required", htmlBody, plainBody); }
                        catch (System.Exception ex) { Console.WriteLine($"Failed to send refund OTP email: {ex.Message}"); }
                    }
                }
                else if (dto.StatusId == 4) // Completed
                {
                    if (booking != null)
                    {
                        var cancelledBookingStatus = await _context.BookingStatuses.FirstOrDefaultAsync(bs => bs.BookingStatusName == "Cancelled");
                        if (cancelledBookingStatus != null)
                        {
                            booking.BookingStatusId = cancelledBookingStatus.BookingStatusId;
                        }
                        else
                        {
                            booking.BookingStatusId = 3;
                        }
                        booking.CancelReason = "Cancelled via Manual Refund Completion";

                        var fullBooking = await _context.Bookings
                            .Include(b => b.BookingSeats)
                            .Include(b => b.Schedule)
                            .FirstOrDefaultAsync(b => b.BookingID == booking.BookingID);

                        if (fullBooking != null)
                        {
                            var availableSeatStatus = await _context.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
                            if (availableSeatStatus != null && fullBooking.Schedule != null)
                            {
                                var seatIds = fullBooking.BookingSeats.Select(bs => bs.SeatId).ToList();
                                var scheduleSeatStatuses = await _context.ScheduleSeatStatuses
                                    .Where(s => s.ScheduleID == fullBooking.ScheduleID && seatIds.Contains(s.SeatID))
                                    .ToListAsync();

                                foreach (var status in scheduleSeatStatuses)
                                {
                                    status.BookingID = null;
                                    status.SeatStatusId = availableSeatStatus.SeatStatusId;
                                    _context.ScheduleSeatStatuses.Update(status);
                                }
                                
                                fullBooking.Schedule.AvailableSeats += seatIds.Count;
                                _context.Schedules.Update(fullBooking.Schedule);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    if (booking != null && booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
                    {
                        var htmlBody = $@"
<div style=""font-family: 'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0b0f19; color: #e2e8f0; padding: 40px 20px; text-align: center;"">
    <div style=""max-width: 500px; margin: 0 auto; background: linear-gradient(145deg, #111827, #1f2937); padding: 40px; border-radius: 20px; box-shadow: 0 10px 30px rgba(0,0,0,0.5); border: 1px solid #374151;"">
        <h2 style=""color: #3b82f6; font-size: 24px; margin-bottom: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 2px;"">Refund Completed</h2>
        <p style=""color: #9ca3af; font-size: 16px; margin-bottom: 30px; line-height: 1.5;"">Hello <strong style=""color:#e2e8f0;"">{booking.User.FullName}</strong>, the refund for your booking <b>#{booking.BookingCode}</b> has been completed.</p>
        <div style=""background-color: #1e3a8a; padding: 20px; border-radius: 12px; border: 1px solid #2563eb; display: inline-block; margin-bottom: 30px; box-shadow: 0 0 20px rgba(59, 130, 246, 0.2);"">
            <p style=""color: #9ca3af; font-size: 14px; margin: 0 0 5px 0;"">Refund Amount</p>
            <span style=""font-size: 32px; font-weight: bold; color: #bfdbfe;"">৳{refund.Data.RefundAmount:N0}</span>
        </div>
        <p style=""color: #6b7280; font-size: 14px; margin-top: 20px;"">Processed At: {DateTime.UtcNow.ToString("f")} UTC</p>
        <div style=""margin-top: 40px; padding-top: 20px; border-top: 1px solid #374151;"">
            <span style=""color: #3b82f6; font-weight: bold; font-size: 18px;"">Saowari</span><br>
            <span style=""color: #6b7280; font-size: 12px;"">Next-Generation Ticketing</span>
        </div>
    </div>
</div>";
                            
                        var plainBody = $"Your refund of ৳{refund.Data.RefundAmount:N0} for booking {booking.BookingCode} has been completed.";

                        await _emailService.SendEmailAsync(booking.User.Email, "Saowari Refund Completed", htmlBody, plainBody);
                    }
                }
            }
            catch (System.Exception) { /* Fail-safe */ }

            return Ok(ApiResponse<bool>.Ok(true, "Refund status updated successfully."));
        }

        /// <summary>
        /// Admin-only: Reset a refund back to "OTP Pending" state (status=2, IsRefunded=false)
        /// so the customer can still enter the OTP they received by email.
        /// Use this to fix refunds that were auto-completed by the old buggy code.
        /// </summary>
        [HttpPost("{id}/reset-to-otp-pending")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetToOtpPending(int id)
        {
            var refundEntity = await _context.Refunds
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.RefundID == id);

            if (refundEntity == null)
                return NotFound(ApiResponse<bool>.Fail("Refund not found."));

            // Only allow reset if RefundOtpCode is still stored (i.e., OTP was generated)
            if (string.IsNullOrEmpty(refundEntity.RefundOtpCode))
                return BadRequest(ApiResponse<bool>.Fail("No OTP found for this refund. Please re-approve the refund to generate a new OTP."));

            // Extend OTP expiry by 30 more minutes so the customer has time to enter it
            refundEntity.RefundStatusId = 2;   // Approved (OTP Pending)
            refundEntity.IsRefunded = false;    // Not yet completed
            refundEntity.ProcessedAt = null;    // Clear processed date
            refundEntity.RefundOtpExpireTime = System.DateTime.UtcNow.AddMinutes(30); // Extend expiry

            await _context.SaveChangesAsync();

            Console.WriteLine($"\n=======================================================");
            Console.WriteLine($"REFUND RESET TO OTP-PENDING");
            Console.WriteLine($"Refund ID: {id}, OTP still valid: {refundEntity.RefundOtpCode}");
            Console.WriteLine($"=======================================================\n");

            return Ok(ApiResponse<bool>.Ok(true, "Refund reset to OTP-pending. Customer can now enter their OTP."));
        }

        [HttpPost("{id}/verify-otp")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyRefundOtp(int id, [FromBody] RefundVerifyOtpDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var refund = await _context.Refunds
                .Include(r => r.Booking)
                    .ThenInclude(b => b.BookingSeats)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Schedule)
                .FirstOrDefaultAsync(r => r.RefundID == id);

            if (refund == null) return NotFound(ApiResponse<bool>.Fail("Refund not found."));

            if (refund.Booking.UserID != userId)
                return Forbid("You can only verify your own refunds.");

            if (refund.RefundOtpCode != dto.OtpCode || refund.RefundOtpExpireTime < System.DateTime.UtcNow)
                return BadRequest(ApiResponse<bool>.Fail("Invalid or expired OTP."));

            // Verification successful — mark the refund as completed
            refund.IsRefunded = true;
            refund.ProcessedAt = System.DateTime.UtcNow;
            refund.RefundOtpCode = null;
            refund.RefundOtpExpireTime = null;
            refund.RefundStatusId = 4; // Completed
            
            // Cancel the booking and release seats
            var booking = refund.Booking;
            var cancelledBookingStatus = await _context.BookingStatuses.FirstOrDefaultAsync(bs => bs.BookingStatusName == "Cancelled");
            if (cancelledBookingStatus != null)
            {
                booking.BookingStatusId = cancelledBookingStatus.BookingStatusId;
            }
            else
            {
                booking.BookingStatusId = 3;
            }
            booking.CancelReason = "Cancelled via Refund Verification";

            var seatStatuses = await _context.ScheduleSeatStatuses
                .Where(sss => sss.BookingID == booking.BookingID)
                .ToListAsync();

            var availableSeatStatus = await _context.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
            if (availableSeatStatus != null && booking.Schedule != null)
            {
                var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
                foreach (var ss in seatStatuses)
                {
                    ss.BookingID = null;
                    ss.SeatStatusId = availableSeatStatus.SeatStatusId;
                }
                
                // Restore available seats count
                booking.Schedule.AvailableSeats += seatIds.Count;
                _context.Schedules.Update(booking.Schedule);
            }

            await _context.SaveChangesAsync();

            // Push real-time notification to customer
            try
            {
                await _notificationService.CreateForUserAsync(
                    userId,
                    "Refund Completed!",
                    $"Your refund of \u09f3{refund.RefundAmount:N0} for booking #{refund.Booking.BookingCode} is now fully processed. Thank you!",
                    "refund", "fas fa-check-double", "bg-blue-100 text-blue-600",
                    "Refund", refund.RefundID);
            }
            catch (System.Exception ex) { Console.WriteLine($"SignalR notify failed: {ex.Message}"); }

            // Send confirmation email
            var user = await _context.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var htmlBody = $@"
<div style=""font-family: 'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0b0f19; color: #e2e8f0; padding: 40px 20px; text-align: center;"">
    <div style=""max-width: 500px; margin: 0 auto; background: linear-gradient(145deg, #111827, #1f2937); padding: 40px; border-radius: 20px; box-shadow: 0 10px 30px rgba(0,0,0,0.5); border: 1px solid #374151;"">
        <h2 style=""color: #3b82f6; font-size: 24px; margin-bottom: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 2px;"">Refund Verification Success</h2>
        <p style=""color: #9ca3af; font-size: 16px; margin-bottom: 30px; line-height: 1.5;"">Hello <strong style=""color:#e2e8f0;"">{user.FullName}</strong>, you have successfully verified your OTP. Your refund for booking <b>#{refund.Booking.BookingCode}</b> is now fully processed.</p>
        <div style=""background-color: #1e3a8a; padding: 20px; border-radius: 12px; border: 1px solid #2563eb; display: inline-block; margin-bottom: 30px; box-shadow: 0 0 20px rgba(59, 130, 246, 0.2);"">
            <p style=""color: #9ca3af; font-size: 14px; margin: 0 0 5px 0;"">Refund Amount</p>
            <span style=""font-size: 32px; font-weight: bold; color: #bfdbfe;"">৳{refund.RefundAmount:N0}</span>
        </div>
        <p style=""color: #6b7280; font-size: 14px; margin-top: 20px;"">Processed At: {refund.ProcessedAt?.ToString("f")} UTC</p>
        <div style=""margin-top: 40px; padding-top: 20px; border-top: 1px solid #374151;"">
            <span style=""color: #3b82f6; font-weight: bold; font-size: 18px;"">Saowari</span><br>
            <span style=""color: #6b7280; font-size: 12px;"">Next-Generation Ticketing</span>
        </div>
    </div>
</div>";
                var plainBody = $"Your refund of ৳{refund.RefundAmount:N0} for booking {refund.Booking.BookingCode} has been completed successfully.";
                
                try { await _emailService.SendEmailAsync(user.Email, "Saowari Refund Completed", htmlBody, plainBody); }
                catch { /* Fail-safe */ }
            }

            return Ok(ApiResponse<bool>.Ok(true, "Refund processed successfully."));
        }
    }
}