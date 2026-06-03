using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/schedule-applications")]
    [ApiController]
    [Authorize]
    public class ScheduleApplicationsController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public ScheduleApplicationsController(SaowariDbContext context)
        {
            _context = context;
        }

        /// <summary>Get applications. Manager sees company's apps; Driver/Supervisor sees own.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<ScheduleApplication> query = _context.ScheduleApplications
                .Include(a => a.Requester)
                .Include(a => a.Route).ThenInclude(r => r.FromLocation)
                .Include(a => a.Route).ThenInclude(r => r.ToLocation)
                .Include(a => a.Vehicle)
                .Include(a => a.Company)
                .AsQueryable();

            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId))
                    return BadRequest(ApiResponse<object>.Fail("Company ID not found in token."));
                query = query.Where(a => a.CompanyId == companyId);
            }
            else if (userRole == "Driver" || userRole == "Supervisor")
            {
                query = query.Where(a => a.RequesterId == userId);
            }
            else if (userRole != "Admin" && userRole != "Agent")
            {
                return Forbid();
            }

            var apps = await query.OrderByDescending(a => a.CreatedAt).Select(a => new
            {
                a.Id,
                a.RequesterId,
                RequesterName = a.Requester.FullName,
                a.CompanyId,
                CompanyName = a.Company.CompanyName,
                a.RouteId,
                RouteName = $"{a.Route.FromLocation.LocationName} → {a.Route.ToLocation.LocationName}",
                a.VehicleId,
                VehicleName = a.Vehicle.VehicleName,
                VehicleNumber = a.Vehicle.VehicleNumber,
                a.DepartureDateTime,
                a.ArrivalDateTime,
                a.Status,
                a.Remarks,
                a.ManagerRemarks,
                a.CreatedAt,
                a.RespondedAt,
                a.CreatedScheduleId
            }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(apps));
        }

        /// <summary>Driver/Supervisor applies for a schedule.</summary>
        [HttpPost]
        [Authorize(Roles = "Driver,Supervisor")]
        public async Task<IActionResult> Create([FromBody] ScheduleApplicationCreateDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            // Find the requester to get their company
            var requester = await _context.Users.FindAsync(userId);
            if (requester == null) return NotFound(ApiResponse<object>.Fail("User not found."));
            if (!requester.CompanyId.HasValue)
                return BadRequest(ApiResponse<object>.Fail("You are not associated with a company."));

            // Validate vehicle belongs to same company
            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null || vehicle.CompanyId != requester.CompanyId.Value)
                return BadRequest(ApiResponse<object>.Fail("Vehicle does not belong to your company."));

            var app = new ScheduleApplication
            {
                RequesterId = userId,
                CompanyId = requester.CompanyId.Value,
                RouteId = dto.RouteId,
                VehicleId = dto.VehicleId,
                DepartureDateTime = dto.DepartureDateTime,
                ArrivalDateTime = dto.ArrivalDateTime,
                Remarks = dto.Remarks,
                Status = "Pending"
            };

            _context.ScheduleApplications.Add(app);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { app.Id }, "Schedule application submitted successfully."));
        }

        /// <summary>Company Manager approves or rejects a schedule application.</summary>
        [HttpPatch("{id}/respond")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> Respond(int id, [FromBody] ScheduleApplicationRespondDto dto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var app = await _context.ScheduleApplications
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound(ApiResponse<object>.Fail("Application not found."));

            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!int.TryParse(companyIdClaim, out int companyId) || app.CompanyId != companyId)
                    return Forbid("You can only manage applications for your own company.");
            }

            if (app.Status != "Pending")
                return BadRequest(ApiResponse<object>.Fail($"Application is already {app.Status}."));

            var validStatuses = new[] { "Approved", "Rejected" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(ApiResponse<object>.Fail("Status must be 'Approved' or 'Rejected'."));

            app.Status = dto.Status;
            app.ManagerRemarks = dto.ManagerRemarks;
            app.RespondedAt = DateTime.UtcNow;

            // If approved, auto-create the schedule
            if (dto.Status == "Approved")
            {
                var activeStatus = await _context.ScheduleStatuses
                    .FirstOrDefaultAsync(s => s.ScheduleStatusName == "Scheduled");

                var requester = await _context.Users.FindAsync(app.RequesterId);
                int? driverId = requester?.DriverInformtionId;
                int? supervisorId = requester?.SupervisorId;

                var newSchedule = new Schedule
                {
                    RouteId = app.RouteId,
                    VehicleId = app.VehicleId,
                    DepartureDateTime = app.DepartureDateTime,
                    ArrivalDateTime = app.ArrivalDateTime,
                    ScheduleStatusId = activeStatus?.ScheduleStatusId ?? 1,
                    DriverInformtionId = driverId ?? 0,
                    SupervisorId = supervisorId,
                    BasePrice = 0,
                    AvailableSeats = app.Vehicle.TotalSeats
                };

                _context.Schedules.Add(newSchedule);
                await _context.SaveChangesAsync();

                app.CreatedScheduleId = newSchedule.ScheduleID;
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, $"Application {dto.Status.ToLower()} successfully."));
        }
    }

    public class ScheduleApplicationCreateDto
    {
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string? Remarks { get; set; }
    }

    public class ScheduleApplicationRespondDto
    {
        public string Status { get; set; } = string.Empty; // Approved or Rejected
        public string? ManagerRemarks { get; set; }
    }
}
