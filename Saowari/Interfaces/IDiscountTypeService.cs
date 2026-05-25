using Saowari.Models.DTOs.DiscountType;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IDiscountTypeService
    {
        Task<ApiResponse<IEnumerable<DiscountTypeResponseDto>>> GetAllAsync();
        Task<ApiResponse<DiscountTypeResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<DiscountTypeResponseDto>> CreateAsync(DiscountTypeCreateDto dto);
        Task<ApiResponse<DiscountTypeResponseDto>> UpdateAsync(int id, DiscountTypeUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}