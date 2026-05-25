using Saowari.Models.DTOs.ScheduleStatus;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IScheduleStatusService
    {
        Task<ApiResponse<IEnumerable<ScheduleStatusResponseDto>>> GetAllAsync();
        Task<ApiResponse<ScheduleStatusResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<ScheduleStatusResponseDto>> CreateAsync(ScheduleStatusCreateDto dto);
        Task<ApiResponse<ScheduleStatusResponseDto>> UpdateAsync(int id, ScheduleStatusUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}