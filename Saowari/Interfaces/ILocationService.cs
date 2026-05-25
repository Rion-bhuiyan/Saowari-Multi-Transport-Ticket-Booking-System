using Saowari.Models.DTOs.Location;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ILocationService
    {
        Task<ApiResponse<IEnumerable<LocationResponseDto>>> GetAllAsync();
        Task<ApiResponse<LocationResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<LocationResponseDto>> CreateAsync(LocationCreateDto dto);
        Task<ApiResponse<LocationResponseDto>> UpdateAsync(int id, LocationUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}