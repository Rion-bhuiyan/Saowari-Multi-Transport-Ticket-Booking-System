using Saowari.Models.DTOs.PaymentMethod;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<ApiResponse<IEnumerable<PaymentMethodResponseDto>>> GetAllAsync();
        Task<ApiResponse<IEnumerable<PaymentMethodResponseDto>>> GetActiveAsync();
        Task<ApiResponse<PaymentMethodResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaymentMethodResponseDto>> CreateAsync(PaymentMethodCreateDto dto);
        Task<ApiResponse<PaymentMethodResponseDto>> UpdateAsync(int id, PaymentMethodUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}