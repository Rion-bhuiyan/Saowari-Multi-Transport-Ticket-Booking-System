using Saowari.Models.DTOs.SeatClass;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ISeatClassService
    {
        Task<ApiResponse<IEnumerable<SeatClassResponseDto>>> GetAllAsync();
        Task<ApiResponse<SeatClassResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<SeatClassResponseDto>> CreateAsync(SeatClassCreateDto dto);
        Task<ApiResponse<SeatClassResponseDto>> UpdateAsync(int id, SeatClassUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}