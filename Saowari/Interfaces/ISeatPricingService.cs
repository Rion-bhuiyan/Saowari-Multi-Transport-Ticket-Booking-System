using Saowari.Models.DTOs.SeatPricing;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ISeatPricingService
    {
        Task<ApiResponse<IEnumerable<SeatPricingResponseDto>>> GetAllAsync();
        Task<ApiResponse<SeatPricingResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<SeatPricingResponseDto>> CreateAsync(SeatPricingCreateDto dto);
        Task<ApiResponse<SeatPricingResponseDto>> UpdateAsync(int id, SeatPricingUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<SeatPricingResponseDto>>> GetByVehicleIdAsync(int vehicleId);
        Task<ApiResponse<bool>> BulkUpsertForVehicleAsync(int vehicleId, List<SeatClassPricingInputDto> pricings);
    }
}