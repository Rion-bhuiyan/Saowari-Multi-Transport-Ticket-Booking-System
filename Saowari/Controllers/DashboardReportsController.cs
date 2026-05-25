using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOrManager")] // Admins or Managers can view reports
    public class DashboardReportsController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public DashboardReportsController(SaowariDbContext context)
        {
            _context = context;
        }

        // GET: api/DashboardReports/revenue
        [HttpGet("revenue")]
        public async Task<ActionResult<ApiResponse<object>>> GetRevenueReport(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate,
            [FromQuery] int? companyId)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int managedCompanyId))
                {
                    companyId = managedCompanyId;
                }
                else
                {
                    return BadRequest(ApiResponse<object>.Fail("Company ID claim is missing or invalid."));
                }
            }

            var query = _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Schedule)
                        .ThenInclude(s => s.Vehicle)
                .Where(p => p.PaymentStatus.PaymentStatusName == "Completed" || p.PaymentStatus.PaymentStatusName == "Paid");

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(p => p.CreatedAt >= start);
            }
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= end);
            }
            if (companyId.HasValue)
            {
                query = query.Where(p => p.Booking != null && 
                                         p.Booking.Schedule != null && 
                                         p.Booking.Schedule.Vehicle != null && 
                                         p.Booking.Schedule.Vehicle.CompanyId == companyId.Value);
            }

            var payments = await query.ToListAsync();

            // Group by Date in memory (safe across all EF Core database providers)
            var grouped = payments
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Select(p => p.BookingId).Distinct().Count(),
                    Revenue = g.Sum(p => p.Amount)
                })
                .OrderBy(g => g.Date)
                .ToList();

            return Ok(ApiResponse<object>.Ok(grouped, "Revenue report retrieved successfully."));
        }

        // GET: api/DashboardReports/occupancy
        [HttpGet("occupancy")]
        public async Task<ActionResult<ApiResponse<object>>> GetOccupancyReport([FromQuery] int? companyId)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole == "CompanyManager")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int managedCompanyId))
                {
                    companyId = managedCompanyId;
                }
                else
                {
                    return BadRequest(ApiResponse<object>.Fail("Company ID claim is missing or invalid."));
                }
            }

            // Fetch schedules with related vehicle, route, and bookings to compute occupancy
            var scheduleQuery = _context.Schedules
                .Include(s => s.Vehicle)
                .Include(s => s.Route)
                    .ThenInclude(r => r.FromLocation)
                .Include(s => s.Route)
                    .ThenInclude(r => r.ToLocation)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.BookingStatus)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.BookingSeats)
                .AsQueryable();

            if (companyId.HasValue)
            {
                scheduleQuery = scheduleQuery.Where(s => s.Vehicle != null && s.Vehicle.CompanyId == companyId.Value);
            }

            var schedules = await scheduleQuery.ToListAsync();

            var reports = schedules
                .Select(s =>
                {
                    var totalSeats = s.Vehicle?.TotalSeats ?? 0;
                    var occupiedSeats = s.Bookings
                        .Where(b => b.BookingStatus?.BookingStatusName == "Confirmed")
                        .Sum(b => b.BookingSeats.Count);

                    var routeName = s.Route != null && s.Route.FromLocation != null && s.Route.ToLocation != null
                        ? $"{s.Route.FromLocation.LocationName} to {s.Route.ToLocation.LocationName}"
                        : "Unknown Route";

                    var occupancyRate = totalSeats > 0 ? Math.Round((double)occupiedSeats / totalSeats * 100, 2) : 0;

                    return new
                    {
                        RouteName = routeName,
                        TotalSeats = totalSeats,
                        OccupiedSeats = occupiedSeats,
                        OccupancyRate = occupancyRate
                    };
                })
                .ToList();

            return Ok(ApiResponse<object>.Ok(reports, "Occupancy report retrieved successfully."));
        }
    }
}
