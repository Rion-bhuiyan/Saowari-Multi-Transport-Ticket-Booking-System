using Saowari.Models.DTOs.Payment;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<IEnumerable<PaymentResponseDto>>> GetAllAsync();
        Task<ApiResponse<PaymentResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaymentResponseDto>> CreateAsync(PaymentCreateDto dto);
        Task<ApiResponse<PaymentResponseDto>> UpdateAsync(int id, PaymentUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}