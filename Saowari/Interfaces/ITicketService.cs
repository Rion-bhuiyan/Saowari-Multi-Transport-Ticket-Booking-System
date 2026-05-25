using Saowari.Models.DTOs.Ticket;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ITicketService
    {
        Task<ApiResponse<IEnumerable<TicketResponseDto>>> GetAllAsync();
        Task<ApiResponse<TicketResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<TicketResponseDto>> CreateAsync(TicketCreateDto dto);
        Task<ApiResponse<TicketResponseDto>> UpdateAsync(int id, TicketUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}