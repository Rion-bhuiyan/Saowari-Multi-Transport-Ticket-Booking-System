using Saowari.Models.DTOs.Discount;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IDiscountService
    {
        Task<ApiResponse<IEnumerable<DiscountResponseDto>>> GetAllAsync();
        Task<ApiResponse<DiscountResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<DiscountResponseDto>> CreateAsync(DiscountCreateDto dto);
        Task<ApiResponse<DiscountResponseDto>> UpdateAsync(int id, DiscountUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<CouponValidationResponseDto>> ValidateCouponAsync(CouponValidationRequestDto request);
    }
}