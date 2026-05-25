using Saowari.Models.DTOs.VehicleType;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IVehicleTypeService
    {
        Task<ApiResponse<IEnumerable<VehicleTypeResponseDto>>> GetAllAsync();
        Task<ApiResponse<VehicleTypeResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<VehicleTypeResponseDto>> CreateAsync(VehicleTypeCreateDto dto);
        Task<ApiResponse<VehicleTypeResponseDto>> UpdateAsync(int id, VehicleTypeUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}