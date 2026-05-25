using Saowari.Models.DTOs.Booking;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IBookingService
    {
        Task<ApiResponse<IEnumerable<BookingResponseDto>>> GetAllAsync();
        Task<ApiResponse<IEnumerable<BookingResponseDto>>> GetMyAsync(int userId);
        Task<ApiResponse<BookingResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<TicketDetailsDto>> GetTicketDetailsAsync(int id);
        Task<ApiResponse<TicketDetailsDto>> GetTicketDetailsByCodeAsync(string bookingCode);
        Task<ApiResponse<BookingResponseDto>> CreateAsync(BookingCreateDto dto);
        Task<ApiResponse<BookingResponseDto>> UpdateAsync(int id, BookingUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}