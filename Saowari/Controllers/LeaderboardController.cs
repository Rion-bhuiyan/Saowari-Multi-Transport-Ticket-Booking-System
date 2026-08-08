using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.DTOs.Business;
using Saowari.Models.Responses;
using System.Security.Claims;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOrManager")]
    public class LeaderboardController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public LeaderboardController(SaowariDbContext context)
        {
            _context = context;
        }

        [HttpGet("customers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaderboardCustomerDto>>>> GetCustomers(
            [FromQuery] string timeframe = "all", 
            [FromQuery] string sortBy = "tickets",
            [FromQuery] int? companyId = null)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // If Company Manager, force filter to their company only
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int managerCompanyId))
                {
                    companyId = managerCompanyId;
                }
            }

            var customerRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserRoleName == "Customer");
            var customerRoleId = customerRole?.UserRoleId ?? 3;

            var query = _context.Users
                .Where(u => u.RoleID == customerRoleId) 
                .AsQueryable();

            // Fetch the customers
            var customersList = await query.ToListAsync();
            
            var leaderboard = new List<LeaderboardCustomerDto>();

            // Determine date filter for bookings
            DateTime? startDate = null;
            if (timeframe.ToLower() == "today") startDate = DateTime.UtcNow.Date;
            else if (timeframe.ToLower() == "this_week") startDate = DateTime.UtcNow.Date.AddDays(-7);
            else if (timeframe.ToLower() == "this_month") startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            else if (timeframe.ToLower() == "this_year") startDate = new DateTime(DateTime.UtcNow.Year, 1, 1);

            foreach (var user in customersList)
            {
                // Get Bookings logic
                var userBookingsQuery = _context.Bookings
                    .Include(b => b.Schedule)
                        .ThenInclude(s => s.Vehicle)
                    .Include(b => b.BookingSeats)
                    .Where(b => b.UserID == user.UserID && b.BookingStatusId == 2); // Confirmed

                if (startDate.HasValue)
                {
                    userBookingsQuery = userBookingsQuery.Where(b => b.BookingDate >= startDate.Value);
                }

                if (companyId.HasValue)
                {
                    userBookingsQuery = userBookingsQuery.Where(b => b.Schedule.Vehicle.CompanyId == companyId.Value);
                }

                var bookings = await userBookingsQuery.ToListAsync();

                // Get Global Activity
                var loginHistoriesQuery = _context.UserLoginHistories.Where(h => h.UserId == user.UserID);
                if (startDate.HasValue)
                {
                    loginHistoriesQuery = loginHistoriesQuery.Where(h => h.LoginTime >= startDate.Value);
                }
                
                var loginHistories = await loginHistoriesQuery.ToListAsync();

                var totalTickets = bookings.Sum(b => b.BookingSeats?.Count ?? 0);
                var totalSpent = bookings.Sum(b => b.FinalAmount);
                var totalLogins = loginHistories.Count;
                var totalTime = loginHistories.Sum(h => h.SessionDurationMinutes);

                // Include if they have any stats (otherwise skip empty users to keep leaderboard clean)
                if (totalTickets > 0 || totalLogins > 0)
                {
                    leaderboard.Add(new LeaderboardCustomerDto
                    {
                        UserId = user.UserID,
                        Name = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        Picture = user.Picture,
                        TotalTickets = totalTickets,
                        TotalSpent = totalSpent,
                        TotalLogins = totalLogins,
                        TotalTimeSpentMinutes = totalTime
                    });
                }
            }

            // Sorting
            IEnumerable<LeaderboardCustomerDto> sortedList = leaderboard;
            if (sortBy.ToLower() == "tickets")
                sortedList = leaderboard.OrderByDescending(x => x.TotalTickets);
            else if (sortBy.ToLower() == "spent")
                sortedList = leaderboard.OrderByDescending(x => x.TotalSpent);
            else if (sortBy.ToLower() == "logins")
                sortedList = leaderboard.OrderByDescending(x => x.TotalLogins);
            else if (sortBy.ToLower() == "time")
                sortedList = leaderboard.OrderByDescending(x => x.TotalTimeSpentMinutes);

            // Take top 50
            return Ok(ApiResponse<IEnumerable<LeaderboardCustomerDto>>.Ok(sortedList.Take(50).ToList()));
        }
    }
}
