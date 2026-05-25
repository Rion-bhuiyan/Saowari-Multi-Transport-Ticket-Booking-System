using Saowari.Models.DTOs.RefundPolicy;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IRefundPolicyService
    {
        Task<ApiResponse<IEnumerable<RefundPolicyResponseDto>>> GetAllAsync();
        Task<ApiResponse<RefundPolicyResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<RefundPolicyResponseDto>> CreateAsync(RefundPolicyCreateDto dto);
        Task<ApiResponse<RefundPolicyResponseDto>> UpdateAsync(int id, RefundPolicyUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}