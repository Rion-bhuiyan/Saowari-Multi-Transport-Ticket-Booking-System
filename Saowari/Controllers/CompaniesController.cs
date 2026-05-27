using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Company;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _service;
        private readonly IWebHostEnvironment _env;

        public CompaniesController(ICompanyService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<CompanyResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CompanyResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<CompanyResponseDto>>> Create([FromForm] CompanyCreateDto dto)
        {
            if (dto.LogoFile != null)
            {
                dto.LogoURL = await SaveLogoFileAsync(dto.LogoFile);
            }
            if (dto.TicketBackgroundImage != null)
            {
                dto.TicketBackgroundUrl = await SaveLogoFileAsync(dto.TicketBackgroundImage);
            }

            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = 0 }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<CompanyResponseDto>>> Update(int id, [FromForm] CompanyUpdateDto dto)
        {
            if (dto.LogoFile != null)
            {
                dto.LogoURL = await SaveLogoFileAsync(dto.LogoFile);
            }
            if (dto.TicketBackgroundImage != null)
            {
                dto.TicketBackgroundUrl = await SaveLogoFileAsync(dto.TicketBackgroundImage);
            }

            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        private async Task<string> SaveLogoFileAsync(IFormFile file)
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            var uploadsFolder = Path.Combine(webRoot, "uploads", "companies");
            
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
            return $"{baseUrl}/uploads/companies/{uniqueFileName}";
        }
    }
}