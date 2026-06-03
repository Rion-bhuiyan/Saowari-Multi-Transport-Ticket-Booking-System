using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.DTOs.Banner;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/banners")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly SaowariDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BannersController(SaowariDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<BannerResponseDto>>>> GetActiveBanners()
        {
            var banners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BannerResponseDto
                {
                    BannerId = b.BannerId,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    LinkUrl = b.LinkUrl,
                    Position = b.Position,
                    SizeTemplate = b.SizeTemplate,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<BannerResponseDto>>.Ok(banners));
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BannerResponseDto>>>> GetAllBanners()
        {
            var banners = await _context.Banners
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BannerResponseDto
                {
                    BannerId = b.BannerId,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    LinkUrl = b.LinkUrl,
                    Position = b.Position,
                    SizeTemplate = b.SizeTemplate,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<BannerResponseDto>>.Ok(banners));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<BannerResponseDto>>> Create([FromForm] BannerCreateDto dto)
        {
            if (dto.Image == null)
            {
                return BadRequest(ApiResponse<BannerResponseDto>.Fail("Image file is required."));
            }

            var imageUrl = await SaveBannerFileAsync(dto.Image);

            var banner = new Banner
            {
                Title = dto.Title,
                ImageUrl = imageUrl,
                LinkUrl = dto.LinkUrl,
                Position = dto.Position,
                SizeTemplate = dto.SizeTemplate,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            var responseDto = new BannerResponseDto
            {
                BannerId = banner.BannerId,
                Title = banner.Title,
                ImageUrl = banner.ImageUrl,
                LinkUrl = banner.LinkUrl,
                Position = banner.Position,
                SizeTemplate = banner.SizeTemplate,
                IsActive = banner.IsActive,
                CreatedAt = banner.CreatedAt
            };

            return Ok(ApiResponse<BannerResponseDto>.Ok(responseDto, "Banner created successfully"));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<BannerResponseDto>>> Update(int id, [FromForm] BannerUpdateDto dto)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound(ApiResponse<BannerResponseDto>.Fail("Banner not found"));

            if (dto.Image != null)
            {
                // Optionally delete the old file here
                banner.ImageUrl = await SaveBannerFileAsync(dto.Image);
            }

            banner.Title = dto.Title;
            banner.LinkUrl = dto.LinkUrl;
            banner.Position = dto.Position;
            banner.SizeTemplate = dto.SizeTemplate;
            banner.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            var responseDto = new BannerResponseDto
            {
                BannerId = banner.BannerId,
                Title = banner.Title,
                ImageUrl = banner.ImageUrl,
                LinkUrl = banner.LinkUrl,
                Position = banner.Position,
                SizeTemplate = banner.SizeTemplate,
                IsActive = banner.IsActive,
                CreatedAt = banner.CreatedAt
            };

            return Ok(ApiResponse<BannerResponseDto>.Ok(responseDto, "Banner updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound(ApiResponse<bool>.Fail("Banner not found"));

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Banner deleted successfully"));
        }

        private async Task<string> SaveBannerFileAsync(IFormFile file)
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            var uploadsFolder = Path.Combine(webRoot, "uploads", "banners");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}/uploads/banners/{uniqueFileName}";
        }
    }
}
