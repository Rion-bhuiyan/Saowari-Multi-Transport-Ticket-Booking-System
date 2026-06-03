using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saowari.Models.Responses;
using System.IO;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public SettingsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("logo")]
        [AllowAnonymous]
        public IActionResult GetLogo()
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            
            var logoPath = Path.Combine(webRoot, "uploads", "site", "logo.png");
            
            if (System.IO.File.Exists(logoPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                // Append a timestamp to bypass browser caching when the logo is updated
                var fileInfo = new FileInfo(logoPath);
                var lastModified = fileInfo.LastWriteTimeUtc.Ticks;
                
                return Ok(ApiResponse<string>.Ok($"{baseUrl}/uploads/site/logo.png?v={lastModified}"));
            }
            
            return Ok(ApiResponse<string>.Ok(null));
        }

        [HttpPost("logo")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UploadLogo(IFormFile logoFile)
        {
            if (logoFile == null || logoFile.Length == 0)
            {
                return BadRequest(ApiResponse<bool>.Fail("No file uploaded"));
            }

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            
            var uploadsFolder = Path.Combine(webRoot, "uploads", "site");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Always save as logo.png to easily overwrite and reference it globally
            var filePath = Path.Combine(uploadsFolder, "logo.png");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(fileStream);
            }

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            
            return Ok(ApiResponse<string>.Ok($"{baseUrl}/uploads/site/logo.png", "Logo updated successfully"));
        }
        [HttpGet("ticket-background")]
        [AllowAnonymous]
        public IActionResult GetTicketBackground()
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");

            var bgPath = Path.Combine(webRoot, "uploads", "site", "ticket-background.jpg");

            if (System.IO.File.Exists(bgPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                var lastModified = new FileInfo(bgPath).LastWriteTimeUtc.Ticks;
                return Ok(ApiResponse<string>.Ok($"{baseUrl}/uploads/site/ticket-background.jpg?v={lastModified}"));
            }

            return Ok(ApiResponse<string>.Ok(null));
        }

        [HttpPost("ticket-background")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UploadTicketBackground(IFormFile backgroundFile)
        {
            if (backgroundFile == null || backgroundFile.Length == 0)
                return BadRequest(ApiResponse<bool>.Fail("No file uploaded"));

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");

            var uploadsFolder = Path.Combine(webRoot, "uploads", "site");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, "ticket-background.jpg");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await backgroundFile.CopyToAsync(fileStream);
            }

            var req = HttpContext.Request;
            var url = $"{req.Scheme}://{req.Host}{req.PathBase}";
            return Ok(ApiResponse<string>.Ok($"{url}/uploads/site/ticket-background.jpg", "Ticket background updated successfully"));
        }

        [HttpDelete("ticket-background")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteTicketBackground()
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");

            var bgPath = Path.Combine(webRoot, "uploads", "site", "ticket-background.jpg");
            if (System.IO.File.Exists(bgPath))
                System.IO.File.Delete(bgPath);

            return Ok(ApiResponse<bool>.Ok(true, "Ticket background removed"));
        }

        [HttpGet("system")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicSystemSettings([FromServices] Saowari.Data.SaowariDbContext context)
        {
            var settings = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToDictionaryAsync(
                context.SystemSettings, s => s.Key, s => s.Value ?? string.Empty);
            return Ok(ApiResponse<System.Collections.Generic.Dictionary<string, string>>.Ok(settings));
        }
    }
}
