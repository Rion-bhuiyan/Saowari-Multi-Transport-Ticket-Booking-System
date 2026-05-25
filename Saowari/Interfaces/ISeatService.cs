using Saowari.Models.DTOs.Seat;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ISeatService
    {
        Task<ApiResponse<IEnumerable<SeatResponseDto>>> GetAllAsync();
        Task<ApiResponse<SeatResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<SeatResponseDto>> CreateAsync(SeatCreateDto dto);
        Task<ApiResponse<SeatResponseDto>> UpdateAsync(int id, SeatUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}