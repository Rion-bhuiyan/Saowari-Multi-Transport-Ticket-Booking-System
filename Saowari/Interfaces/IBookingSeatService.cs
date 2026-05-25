using Saowari.Models.DTOs.BookingSeat;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IBookingSeatService
    {
        Task<ApiResponse<IEnumerable<BookingSeatResponseDto>>> GetAllAsync();
        Task<ApiResponse<BookingSeatResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<BookingSeatResponseDto>> CreateAsync(BookingSeatCreateDto dto);
        Task<ApiResponse<BookingSeatResponseDto>> UpdateAsync(int id, BookingSeatUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}