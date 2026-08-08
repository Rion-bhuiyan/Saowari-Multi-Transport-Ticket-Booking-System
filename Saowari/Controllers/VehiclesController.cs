using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Vehicle;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _service;

        public VehiclesController(IVehicleService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<VehicleResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.Success) return BadRequest(result);

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    result.Data = result.Data.Where(v => v.CompanyId == companyId).ToList();
                }
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> Create([FromBody] VehicleCreateDto dto)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    dto.CompanyId = companyId; // Force the company manager's company ID
                }
            }

            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.VehicleID }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> Update(int id, [FromBody] VehicleUpdateDto dto)
        {
            // Security check for manager updating another company's vehicle
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var existing = await _service.GetByIdAsync(id);
                    if (existing.Data != null && existing.Data.CompanyId != companyId)
                    {
                        return Forbid("You can only update vehicles belonging to your company.");
                    }
                    dto.CompanyId = companyId;
                }
            }

            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var existing = await _service.GetByIdAsync(id);
                    if (existing.Data != null && existing.Data.CompanyId != companyId)
                    {
                        return Forbid("You can only delete vehicles belonging to your company.");
                    }
                }
            }

            var result = await _service.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{id}/generate-seats")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> GenerateSeats(int id, [FromBody] SeatLayoutConfigDto config)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var existing = await _service.GetByIdAsync(id);
                    if (existing.Data != null && existing.Data.CompanyId != companyId)
                    {
                        return Forbid("You can only generate seats for vehicles belonging to your company.");
                    }
                }
            }

            var result = await _service.GenerateSeatsAsync(id, config);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{id}/seats/classes")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateSeatClasses(int id, [FromBody] List<SeatClassAssignmentDto> assignments)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var existing = await _service.GetByIdAsync(id);
                    if (existing.Data != null && existing.Data.CompanyId != companyId)
                    {
                        return Forbid("You can only configure seats for vehicles belonging to your company.");
                    }
                }
            }

            var result = await _service.UpdateSeatClassesAsync(id, assignments);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
