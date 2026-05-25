using Saowari.Models.DTOs.CompanyType;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ICompanyTypeService
    {
        Task<ApiResponse<IEnumerable<CompanyTypeResponseDto>>> GetAllAsync();
        Task<ApiResponse<CompanyTypeResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<CompanyTypeResponseDto>> CreateAsync(CompanyTypeCreateDto dto);
        Task<ApiResponse<CompanyTypeResponseDto>> UpdateAsync(int id, CompanyTypeUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}