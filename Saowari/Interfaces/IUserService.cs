using Saowari.Models.DTOs.User;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllAsync();
        Task<ApiResponse<UserResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<UserResponseDto>> CreateAsync(UserCreateDto dto);
        Task<ApiResponse<UserResponseDto>> UpdateAsync(int id, UserUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}