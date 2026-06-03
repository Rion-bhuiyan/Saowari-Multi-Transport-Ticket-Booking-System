using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using System.Security.Claims;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public ActivityController(SaowariDbContext context)
        {
            _context = context;
        }

        [HttpPost("ping")]
        public async Task<IActionResult> Ping()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            // Find the latest active login history for this user
            // We order by LoginTime descending to get the most recent session
            var latestSession = await _context.UserLoginHistories
                .Where(h => h.UserId == userId && h.IsActive)
                .OrderByDescending(h => h.LoginTime)
                .FirstOrDefaultAsync();

            if (latestSession != null)
            {
                var lastActive = latestSession.LastActiveTime ?? latestSession.LoginTime;
                var inactiveMinutes = (DateTime.UtcNow - lastActive).TotalMinutes;

                if (inactiveMinutes > 30)
                {
                    latestSession.IsActive = false;
                    
                    var newSession = new Saowari.Models.Entities.UserLoginHistory
                    {
                        UserId = userId,
                        IpAddress = latestSession.IpAddress,
                        DeviceName = latestSession.DeviceName,
                        LoginTime = DateTime.UtcNow,
                        LastActiveTime = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.UserLoginHistories.Add(newSession);
                }
                else
                {
                    latestSession.LastActiveTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound("No active session found.");
        }
    }
}
