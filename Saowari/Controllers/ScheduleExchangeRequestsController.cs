using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/schedule-exchanges")]
    [ApiController]
    [Authorize]
    public class ScheduleExchangeRequestsController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public ScheduleExchangeRequestsController(SaowariDbContext context)
        {
            _context = context;
        }

        /// <summary>Get exchange requests visible to the caller.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<ScheduleExchangeRequest> query = _context.ScheduleExchangeRequests
                .Include(e => e.Requester)
                .Include(e => e.TargetUser)
                .Include(e => e.RequesterSchedule).ThenInclude(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(e => e.RequesterSchedule).ThenInclude(s => s.Route).ThenInclude(r => r.ToLocation)
                .Include(e => e.TargetSchedule).ThenInclude(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(e => e.TargetSchedule).ThenInclude(s => s.Route).ThenInclude(r => r.ToLocation)
                .AsQueryable();

            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId))
                    return BadRequest(ApiResponse<object>.Fail("Company ID not found in token."));
                // Manager sees exchanges where both schedules belong to their company's vehicles
                var companyVehicleIds = await _context.Vehicles
                    .Where(v => v.CompanyId == companyId).Select(v => v.VehicleID).ToListAsync();
                query = query.Where(e =>
                    companyVehicleIds.Contains(e.RequesterSchedule.VehicleId));
            }
            else if (userRole == "Driver" || userRole == "Supervisor")
            {
                query = query.Where(e => e.RequesterId == userId || e.TargetUserId == userId);
            }
            else if (userRole != "Admin" && userRole != "Agent")
            {
                return Forbid();
            }

            var results = await query.OrderByDescending(e => e.CreatedAt).Select(e => new
            {
                e.Id,
                e.RequesterId,
                RequesterName = e.Requester.FullName,
                e.TargetUserId,
                TargetUserName = e.TargetUser.FullName,
                e.RequesterScheduleId,
                RequesterScheduleRoute = $"{e.RequesterSchedule.Route.FromLocation.LocationName} → {e.RequesterSchedule.Route.ToLocation.LocationName}",
                RequesterScheduleDeparture = e.RequesterSchedule.DepartureDateTime,
                e.TargetScheduleId,
                TargetScheduleRoute = $"{e.TargetSchedule.Route.FromLocation.LocationName} → {e.TargetSchedule.Route.ToLocation.LocationName}",
                TargetScheduleDeparture = e.TargetSchedule.DepartureDateTime,
                e.Status,
                e.Remarks,
                e.ManagerRemarks,
                e.CreatedAt,
                e.PeerRespondedAt,
                e.ManagerRespondedAt
            }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(results));
        }

        /// <summary>Driver/Supervisor creates an exchange request.</summary>
        [HttpPost]
        [Authorize(Roles = "Driver,Supervisor")]
        public async Task<IActionResult> Create([FromBody] ScheduleExchangeCreateDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            if (dto.TargetUserId == userId)
                return BadRequest(ApiResponse<object>.Fail("Cannot exchange with yourself."));

            // Validate requester owns their schedule (as driver or supervisor)
            var requesterSchedule = await _context.Schedules
                .Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.ScheduleID == dto.RequesterScheduleId);
            if (requesterSchedule == null)
                return NotFound(ApiResponse<object>.Fail("Your schedule not found."));

            var requester = await _context.Users.FindAsync(userId);
            bool isRequesterDriver = requester?.DriverInformtionId == requesterSchedule.DriverInformtionId &&
                requesterSchedule.DriverInformtionId != 0;
            bool isRequesterSupervisor = requesterSchedule.SupervisorId.HasValue &&
                requester?.SupervisorId == requesterSchedule.SupervisorId;

            if (!isRequesterDriver && !isRequesterSupervisor)
                return Forbid("You are not assigned to that schedule.");

            var targetSchedule = await _context.Schedules.FindAsync(dto.TargetScheduleId);
            if (targetSchedule == null)
                return NotFound(ApiResponse<object>.Fail("Target schedule not found."));

            // Same company check
            if (requesterSchedule.Vehicle?.CompanyId != targetSchedule.VehicleId.ToString() as object as int?)
            {
                // Just ensure target vehicle is same company
                var targetVehicle = await _context.Vehicles.FindAsync(targetSchedule.VehicleId);
                if (targetVehicle?.CompanyId != requesterSchedule.Vehicle?.CompanyId)
                    return BadRequest(ApiResponse<object>.Fail("Cannot exchange schedules across different companies."));
            }

            var request = new ScheduleExchangeRequest
            {
                RequesterId = userId,
                RequesterScheduleId = dto.RequesterScheduleId,
                TargetUserId = dto.TargetUserId,
                TargetScheduleId = dto.TargetScheduleId,
                Remarks = dto.Remarks,
                Status = "Pending"
            };

            _context.ScheduleExchangeRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { request.Id }, "Exchange request submitted successfully."));
        }

        /// <summary>Target peer accepts/rejects the exchange request.</summary>
        [HttpPatch("{id}/peer-respond")]
        [Authorize(Roles = "Driver,Supervisor")]
        public async Task<IActionResult> PeerRespond(int id, [FromBody] ExchangePeerRespondDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var request = await _context.ScheduleExchangeRequests.FindAsync(id);
            if (request == null) return NotFound(ApiResponse<object>.Fail("Request not found."));
            if (request.TargetUserId != userId) return Forbid("You are not the target of this request.");
            if (request.Status != "Pending")
                return BadRequest(ApiResponse<object>.Fail($"Request is already {request.Status}."));

            request.Status = dto.Accept ? "AcceptedByPeer" : "RejectedByPeer";
            request.PeerRespondedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, $"Request {(dto.Accept ? "accepted" : "rejected")}."));
        }

        /// <summary>Company Manager gives final approval/rejection and swaps schedules if approved.</summary>
        [HttpPatch("{id}/manager-respond")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ManagerRespond(int id, [FromBody] ExchangeManagerRespondDto dto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var request = await _context.ScheduleExchangeRequests
                .Include(e => e.RequesterSchedule).ThenInclude(s => s.Vehicle)
                .Include(e => e.TargetSchedule)
                .Include(e => e.Requester)
                .Include(e => e.TargetUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (request == null) return NotFound(ApiResponse<object>.Fail("Request not found."));
            if (request.Status != "AcceptedByPeer")
                return BadRequest(ApiResponse<object>.Fail("Can only finalize requests accepted by the peer."));

            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId) ||
                    request.RequesterSchedule.Vehicle?.CompanyId != companyId)
                    return Forbid("This exchange does not belong to your company.");
            }

            var validStatuses = new[] { "Approved", "Rejected" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(ApiResponse<object>.Fail("Status must be 'Approved' or 'Rejected'."));

            request.Status = dto.Status;
            request.ManagerRemarks = dto.ManagerRemarks;
            request.ManagerRespondedAt = DateTime.UtcNow;

            // If approved, swap driver/supervisor assignments
            if (dto.Status == "Approved")
            {
                var rSchedule = await _context.Schedules.FindAsync(request.RequesterScheduleId);
                var tSchedule = await _context.Schedules.FindAsync(request.TargetScheduleId);

                if (rSchedule != null && tSchedule != null)
                {
                    (rSchedule.DriverInformtionId, tSchedule.DriverInformtionId) =
                        (tSchedule.DriverInformtionId, rSchedule.DriverInformtionId);
                    (rSchedule.SupervisorId, tSchedule.SupervisorId) =
                        (tSchedule.SupervisorId, rSchedule.SupervisorId);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, $"Exchange {dto.Status.ToLower()} successfully."));
        }
    }

    public class ScheduleExchangeCreateDto
    {
        public int RequesterScheduleId { get; set; }
        public int TargetUserId { get; set; }
        public int TargetScheduleId { get; set; }
        public string? Remarks { get; set; }
    }

    public class ExchangePeerRespondDto
    {
        public bool Accept { get; set; }
    }

    public class ExchangeManagerRespondDto
    {
        public string Status { get; set; } = string.Empty;
        public string? ManagerRemarks { get; set; }
    }
}
