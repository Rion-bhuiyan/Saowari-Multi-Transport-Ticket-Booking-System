using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminSettingsController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public AdminSettingsController(SaowariDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<Dictionary<string, string>>>> GetSettings()
        {
            var settings = await _context.SystemSettings.ToDictionaryAsync(s => s.Key, s => s.Value ?? string.Empty);
            return Ok(ApiResponse<Dictionary<string, string>>.Ok(settings));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateSettings([FromBody] Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == kvp.Key);
                if (setting == null)
                {
                    _context.SystemSettings.Add(new SystemSetting { Key = kvp.Key, Value = kvp.Value });
                }
                else
                {
                    setting.Value = kvp.Value;
                    _context.SystemSettings.Update(setting);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Settings updated successfully"));
        }
    }
}
