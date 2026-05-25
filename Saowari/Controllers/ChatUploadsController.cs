using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatUploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedDocExtensions = { ".pdf", ".doc", ".docx" };
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] AllowedAudioExtensions = { ".webm", ".mp3", ".wav", ".ogg", ".m4a" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".avi", ".mkv", ".webm" };

        public ChatUploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string fileType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was received.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            // ── VALIDATION RULES ──────────────────────────────────────────────────────

            if (fileType == "image")
            {
                if (!AllowedImageExtensions.Contains(extension))
                {
                    return BadRequest("Invalid image format. Supported formats: JPG, JPEG, PNG, GIF, WEBP.");
                }
                // 5 MB limit
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Image size exceeds the maximum limit of 5 MB.");
                }
            }
            else if (fileType == "video")
            {
                if (!AllowedVideoExtensions.Contains(extension))
                {
                    return BadRequest("Invalid video format. Supported formats: MP4, MOV, AVI, MKV.");
                }
                // Minimum size: 20 MB
                if (file.Length < 20 * 1024 * 1024)
                {
                    return BadRequest("Video size must be at least 20 MB to be sent.");
                }
            }
            else if (fileType == "pdf" || fileType == "word" || fileType == "document")
            {
                if (!AllowedDocExtensions.Contains(extension))
                {
                    return BadRequest("Invalid document format. Supported formats: PDF, DOC, DOCX.");
                }
            }
            else if (fileType == "voice")
            {
                if (!AllowedAudioExtensions.Contains(extension))
                {
                    return BadRequest("Invalid voice audio format.");
                }
            }
            else
            {
                return BadRequest("Unsupported media channel type.");
            }

            // ── SAVE TO DISK ──────────────────────────────────────────────────────────

            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "chat");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Construct client fetch URL
            var relativeUrl = $"/uploads/chat/{uniqueFileName}";
            return Ok(new { fileUrl = relativeUrl });
        }
    }
}
