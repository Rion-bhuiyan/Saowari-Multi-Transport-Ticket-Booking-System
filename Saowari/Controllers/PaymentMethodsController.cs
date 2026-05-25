using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.PaymentMethod;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/paymentmethods")]
    [ApiController]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _service;
        private readonly IWebHostEnvironment _env;

        public PaymentMethodsController(IPaymentMethodService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentMethodResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentMethodResponseDto>>>> GetActive()
        {
            var result = await _service.GetActiveAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PaymentMethodResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<PaymentMethodResponseDto>>> Create([FromForm] PaymentMethodCreateDto dto)
        {
            if (dto.LogoFile != null)
                dto.LogoUrl = await SaveLogoAsync(dto.LogoFile);

            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.PaymentMethodId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<PaymentMethodResponseDto>>> Update(int id, [FromForm] PaymentMethodUpdateDto dto)
        {
            if (dto.LogoFile != null)
                dto.LogoUrl = await SaveLogoAsync(dto.LogoFile);

            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success)
            {
                if (result.Message != null && result.Message.Contains("not found")) 
                    return NotFound(result);
                return BadRequest(result);
            }
            return Ok(result);
        }

        // ── Helper ──────────────────────────────────────────────────────────────
        private async Task<string> SaveLogoAsync(IFormFile file)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "payment-methods");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var req = HttpContext.Request;
            return $"{req.Scheme}://{req.Host}{req.PathBase}/uploads/payment-methods/{uniqueFileName}";
        }
    }
}