using Saowari.Models.DTOs.Route;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IRouteService
    {
        Task<ApiResponse<IEnumerable<RouteResponseDto>>> GetAllAsync();
        Task<ApiResponse<RouteResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<RouteResponseDto>> CreateAsync(RouteCreateDto dto);
        Task<ApiResponse<RouteResponseDto>> UpdateAsync(int id, RouteUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}