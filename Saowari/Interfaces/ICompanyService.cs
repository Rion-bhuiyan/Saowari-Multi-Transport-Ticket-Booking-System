using Saowari.Models.DTOs.Company;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ICompanyService
    {
        Task<ApiResponse<IEnumerable<CompanyResponseDto>>> GetAllAsync();
        Task<ApiResponse<CompanyResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<CompanyResponseDto>> CreateAsync(CompanyCreateDto dto);
        Task<ApiResponse<CompanyResponseDto>> UpdateAsync(int id, CompanyUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}