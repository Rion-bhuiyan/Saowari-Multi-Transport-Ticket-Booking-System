using Saowari.Models.DTOs.ScheduleSeatStatus;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IScheduleSeatStatusService
    {
        Task<ApiResponse<IEnumerable<ScheduleSeatStatusResponseDto>>> GetAllAsync();
        Task<ApiResponse<ScheduleSeatStatusResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<ScheduleSeatStatusResponseDto>> CreateAsync(ScheduleSeatStatusCreateDto dto);
        Task<ApiResponse<ScheduleSeatStatusResponseDto>> UpdateAsync(int id, ScheduleSeatStatusUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}