using Saowari.Models.DTOs.Refund;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IRefundService
    {
        Task<ApiResponse<IEnumerable<RefundResponseDto>>> GetAllAsync();
        Task<ApiResponse<RefundResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<RefundResponseDto>> CreateAsync(RefundCreateDto dto);
        Task<ApiResponse<RefundResponseDto>> UpdateAsync(int id, RefundUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}