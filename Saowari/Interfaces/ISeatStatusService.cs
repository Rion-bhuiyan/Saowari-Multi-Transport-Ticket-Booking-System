using Saowari.Models.DTOs.SeatStatus;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ISeatStatusService
    {
        Task<ApiResponse<IEnumerable<SeatStatusResponseDto>>> GetAllAsync();
        Task<ApiResponse<SeatStatusResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<SeatStatusResponseDto>> CreateAsync(SeatStatusCreateDto dto);
        Task<ApiResponse<SeatStatusResponseDto>> UpdateAsync(int id, SeatStatusUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}