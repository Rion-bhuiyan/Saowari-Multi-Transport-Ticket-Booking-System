using Saowari.Models.DTOs.BookingStatus;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IBookingStatusService
    {
        Task<ApiResponse<IEnumerable<BookingStatusResponseDto>>> GetAllAsync();
        Task<ApiResponse<BookingStatusResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<BookingStatusResponseDto>> CreateAsync(BookingStatusCreateDto dto);
        Task<ApiResponse<BookingStatusResponseDto>> UpdateAsync(int id, BookingStatusUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}