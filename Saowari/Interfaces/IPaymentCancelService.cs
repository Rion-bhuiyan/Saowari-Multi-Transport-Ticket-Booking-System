using Saowari.Models.DTOs.PaymentCancel;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IPaymentCancelService
    {
        Task<ApiResponse<IEnumerable<PaymentCancelResponseDto>>> GetAllAsync();
        Task<ApiResponse<PaymentCancelResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaymentCancelResponseDto>> CreateAsync(PaymentCancelCreateDto dto);
        Task<ApiResponse<PaymentCancelResponseDto>> UpdateAsync(int id, PaymentCancelUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}