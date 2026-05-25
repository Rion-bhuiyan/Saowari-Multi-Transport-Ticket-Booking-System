using Saowari.Models.DTOs.DriverInformtion;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IDriverInformtionService
    {
        Task<ApiResponse<IEnumerable<DriverInformtionResponseDto>>> GetAllAsync();
        Task<ApiResponse<DriverInformtionResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<DriverInformtionResponseDto>> CreateAsync(DriverInformtionCreateDto dto);
        Task<ApiResponse<DriverInformtionResponseDto>> UpdateAsync(int id, DriverInformtionUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}