using Saowari.Models.DTOs.Supervisor;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ISupervisorService
    {
        Task<ApiResponse<IEnumerable<SupervisorResponseDto>>> GetAllAsync();
        Task<ApiResponse<SupervisorResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<SupervisorResponseDto>> CreateAsync(SupervisorCreateDto dto);
        Task<ApiResponse<SupervisorResponseDto>> UpdateAsync(int id, SupervisorUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}