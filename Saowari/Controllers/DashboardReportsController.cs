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

            if ((userRole == "CompanyManager" || userRole == "Manager"))
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

            if ((userRole == "CompanyManager" || userRole == "Manager"))
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

        // GET: api/DashboardReports/advanced-analytics
        [HttpGet("advanced-analytics")]
        public async Task<ActionResult<ApiResponse<object>>> GetAdvancedAnalytics(
            [FromQuery] int? companyId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if ((userRole == "CompanyManager" || userRole == "Manager"))
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

            var now = DateTime.UtcNow;
            
            // Default to today/this month if no custom dates
            var currentStart = startDate?.Date ?? now.Date;
            var currentEnd = endDate?.Date ?? now.Date;
            var durationDays = (currentEnd - currentStart).Days + 1;
            
            // Previous period is the exact same duration immediately preceding the selected period
            var previousStart = currentStart.AddDays(-durationDays);
            var previousEnd = currentStart.AddDays(-1);

            // Legacy defaults for the second card (This Month vs Last Month) if no custom dates selected
            var firstDayThisMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
            var lastDayLastMonth = firstDayThisMonth.AddDays(-1);

            var query = _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Schedule)
                        .ThenInclude(s => s.Vehicle)
                            .ThenInclude(v => v.Company)
                .Where(p => p.PaymentStatus.PaymentStatusName == "Completed" || p.PaymentStatus.PaymentStatusName == "Paid")
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(p => p.Booking != null && 
                                         p.Booking.Schedule != null && 
                                         p.Booking.Schedule.Vehicle != null && 
                                         p.Booking.Schedule.Vehicle.CompanyId == companyId.Value);
            }

            var allPayments = await query.ToListAsync();

            // 1. Period-over-Period Stats (Dynamic based on selected dates)
            // If user selects custom dates, we compare "Selected Period" vs "Previous Period".
            // If no dates selected, it acts as "Today" vs "Yesterday".
            var todayRevenue = allPayments.Where(p => p.CreatedAt.Date >= currentStart && p.CreatedAt.Date <= currentEnd).Sum(p => p.Amount);
            var yesterdayRevenue = allPayments.Where(p => p.CreatedAt.Date >= previousStart && p.CreatedAt.Date <= previousEnd).Sum(p => p.Amount);
            var todayGrowth = yesterdayRevenue > 0 ? Math.Round(((double)(todayRevenue - yesterdayRevenue) / (double)yesterdayRevenue) * 100, 2) : (todayRevenue > 0 ? 100 : 0);

            // Secondary card: Always "This Month vs Last Month" UNLESS custom dates are selected, then we can just return 0 or hide it.
            // Actually, let's keep "This Month" fixed so they always have a monthly benchmark, regardless of the custom day selection.
            var thisMonthRevenue = allPayments.Where(p => p.CreatedAt >= firstDayThisMonth).Sum(p => p.Amount);
            var lastMonthRevenue = allPayments.Where(p => p.CreatedAt >= firstDayLastMonth && p.CreatedAt <= lastDayLastMonth).Sum(p => p.Amount);
            var monthGrowth = lastMonthRevenue > 0 ? Math.Round(((double)(thisMonthRevenue - lastMonthRevenue) / (double)lastMonthRevenue) * 100, 2) : (thisMonthRevenue > 0 ? 100 : 0);

            // 2. Company Comparisons (Constrained to the selected date range)
            var currentPeriodPayments = allPayments.Where(p => p.CreatedAt.Date >= currentStart && p.CreatedAt.Date <= currentEnd).ToList();
            
            // If the selected range is just 1 day (e.g. today), the leaderboard might be empty. 
            // Fallback to "This Month" if they didn't select custom dates and it's just "today".
            var leaderboardPayments = (startDate.HasValue && endDate.HasValue) 
                ? currentPeriodPayments 
                : allPayments.Where(p => p.CreatedAt >= firstDayThisMonth).ToList();

            var companyComparisons = leaderboardPayments
                .Where(p => p.Booking?.Schedule?.Vehicle?.Company != null)
                .GroupBy(p => p.Booking.Schedule.Vehicle.Company.CompanyName)
                .Select(g => new
                {
                    CompanyName = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    Bookings = g.Select(p => p.BookingId).Distinct().Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // 3. Trend Chart (Constrained to selected date range, max 90 days. Default to 30 days if no custom dates)
            var trendStart = (startDate.HasValue && endDate.HasValue) ? currentStart : now.Date.AddDays(-29);
            var trendEnd = (startDate.HasValue && endDate.HasValue) ? currentEnd : now.Date;
            var trendDuration = (trendEnd - trendStart).Days + 1;
            if (trendDuration > 90) { trendDuration = 90; trendStart = trendEnd.AddDays(-89); } // Cap to 90 days for performance

            var trendPayments = allPayments.Where(p => p.CreatedAt.Date >= trendStart && p.CreatedAt.Date <= trendEnd).ToList();
            var trendGrouped = trendPayments
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(p => p.Amount)
                })
                .ToList();

            var filledTrend = new List<object>();
            for (int i = 0; i < trendDuration; i++)
            {
                var d = trendStart.AddDays(i).ToString("yyyy-MM-dd");
                var existing = trendGrouped.FirstOrDefault(t => t.Date == d);
                filledTrend.Add(new
                {
                    Date = d,
                    Revenue = existing?.Revenue ?? 0
                });
            }

            var result = new
            {
                periodStats = new
                {
                    todayRevenue,
                    yesterdayRevenue,
                    todayGrowth,
                    thisMonthRevenue,
                    lastMonthRevenue,
                    monthGrowth,
                    isCustomDate = startDate.HasValue && endDate.HasValue
                },
                companyComparisons,
                trend30Days = filledTrend
            };

            return Ok(ApiResponse<object>.Ok(result, "Advanced analytics retrieved successfully."));
        }
    }
}
