using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.SliderImage;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/slider-images")]
    [ApiController]
    public class SliderImagesController : ControllerBase
    {
        private readonly ISliderImageService _service;
        private readonly IWebHostEnvironment _env;

        public SliderImagesController(ISliderImageService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<SliderImageResponseDto>>>> GetActive()
        {
            var result = await _service.GetAllActiveAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SliderImageResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<SliderImageResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<SliderImageResponseDto>>> Create([FromForm] SliderImageCreateDto dto)
        {
            if (dto.ImageFile != null)
            {
                dto.ImageUrl = await SaveSliderFileAsync(dto.ImageFile);
            }
            else if (string.IsNullOrEmpty(dto.ImageUrl))
            {
                return BadRequest(ApiResponse<SliderImageResponseDto>.Fail("Image file or URL is required."));
            }

            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.SliderImageID }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<SliderImageResponseDto>>> Update(int id, [FromForm] SliderImageUpdateDto dto)
        {
            if (dto.ImageFile != null)
            {
                dto.ImageUrl = await SaveSliderFileAsync(dto.ImageFile);
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

        private async Task<string> SaveSliderFileAsync(IFormFile file)
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            var uploadsFolder = Path.Combine(webRoot, "uploads", "sliders");
            
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
            return $"{baseUrl}/uploads/sliders/{uniqueFileName}";
        }
    }
}
