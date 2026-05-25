using Saowari.Models.DTOs.Vehicle;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IVehicleService
    {
        Task<ApiResponse<IEnumerable<VehicleResponseDto>>> GetAllAsync();
        Task<ApiResponse<VehicleResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<VehicleResponseDto>> CreateAsync(VehicleCreateDto dto);
        Task<ApiResponse<VehicleResponseDto>> UpdateAsync(int id, VehicleUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> GenerateSeatsAsync(int vehicleId, SeatLayoutConfigDto config);
        Task<ApiResponse<bool>> UpdateSeatClassesAsync(int vehicleId, List<SeatClassAssignmentDto> assignments);
    }
}