using Saowari.Models.DTOs.Schedule;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IScheduleService
    {
        Task<ApiResponse<IEnumerable<ScheduleResponseDto>>> GetAllAsync();
        Task<ApiResponse<ScheduleResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<ScheduleResponseDto>> CreateAsync(ScheduleCreateDto dto);
        Task<ApiResponse<ScheduleResponseDto>> UpdateAsync(int id, ScheduleUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<ScheduleLifecycleDto>> GetLifecycleAsync(int? companyId);
        Task<ApiResponse<ScheduleResponseDto>> ChangeStatusAsync(int id, string statusName);
        Task<ApiResponse<ScheduleResponseDto>> CloneAsync(ScheduleCloneDto dto);
    }
}