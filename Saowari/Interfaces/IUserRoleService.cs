using Saowari.Models.DTOs.UserRole;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface IUserRoleService
    {
        Task<ApiResponse<IEnumerable<UserRoleResponseDto>>> GetAllAsync();
        Task<ApiResponse<UserRoleResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<UserRoleResponseDto>> CreateAsync(UserRoleCreateDto dto);
        Task<ApiResponse<UserRoleResponseDto>> UpdateAsync(int id, UserRoleUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}