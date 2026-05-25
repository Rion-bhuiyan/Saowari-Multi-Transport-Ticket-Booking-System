using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Saowari.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public FilesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Proxies static files through the MVC pipeline so they carry proper CORS headers.
        /// Used by the frontend for PDF image generation (html2canvas cross-origin fix).
        /// </summary>
        [HttpGet("proxy")]
        [AllowAnonymous]
        public IActionResult ProxyFile([FromQuery] string path)
        {
            // Basic security: disallow directory traversal
            if (string.IsNullOrWhiteSpace(path) || path.Contains(".."))
                return BadRequest("Invalid path");

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

            // ASP.NET Core already decodes the query string once (%2F→/, %2520→%20).
            // Decode a second time so filenames with spaces work (%20→' ').
            var decodedPath = Uri.UnescapeDataString(path);

            var cleanPath = decodedPath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRoot, cleanPath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".png"  => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif"  => "image/gif",
                ".webp" => "image/webp",
                ".svg"  => "image/svg+xml",
                _       => "application/octet-stream"
            };

            return PhysicalFile(fullPath, contentType);
        }
    }
}
