using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.SeatPricing;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatPricingsController : ControllerBase
    {
        private readonly ISeatPricingService _service;

        public SeatPricingsController(ISeatPricingService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeatPricingResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<SeatPricingResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("vehicle/{vehicleId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeatPricingResponseDto>>>> GetByVehicle(int vehicleId)
        {
            var result = await _service.GetByVehicleIdAsync(vehicleId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SeatPricingResponseDto>>> Create([FromBody] SeatPricingCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.PricingID }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<SeatPricingResponseDto>>> Update(int id, [FromBody] SeatPricingUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPut("vehicle/{vehicleId}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> BulkUpsertForVehicle(int vehicleId, [FromBody] List<SeatClassPricingInputDto> pricings)
        {
            var result = await _service.BulkUpsertForVehicleAsync(vehicleId, pricings);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}