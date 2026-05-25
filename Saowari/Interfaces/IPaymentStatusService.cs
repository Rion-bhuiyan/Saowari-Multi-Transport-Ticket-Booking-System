using Saowari.Models.DTOs.PaymentStatus;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IPaymentStatusService
    {
        Task<ApiResponse<IEnumerable<PaymentStatusResponseDto>>> GetAllAsync();
        Task<ApiResponse<PaymentStatusResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaymentStatusResponseDto>> CreateAsync(PaymentStatusCreateDto dto);
        Task<ApiResponse<PaymentStatusResponseDto>> UpdateAsync(int id, PaymentStatusUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}