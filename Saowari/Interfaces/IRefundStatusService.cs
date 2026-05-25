using Saowari.Models.DTOs.RefundStatus;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IRefundStatusService
    {
        Task<ApiResponse<IEnumerable<RefundStatusResponseDto>>> GetAllAsync();
        Task<ApiResponse<RefundStatusResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<RefundStatusResponseDto>> CreateAsync(RefundStatusCreateDto dto);
        Task<ApiResponse<RefundStatusResponseDto>> UpdateAsync(int id, RefundStatusUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}