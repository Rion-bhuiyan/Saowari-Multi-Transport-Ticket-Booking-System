using Saowari.Models.DTOs.SliderImage;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Interfaces
{
    public interface ISliderImageService
    {
        Task<ApiResponse<IEnumerable<SliderImageResponseDto>>> GetAllActiveAsync();
        Task<ApiResponse<IEnumerable<SliderImageResponseDto>>> GetAllAsync();
        Task<ApiResponse<SliderImageResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<SliderImageResponseDto>> CreateAsync(SliderImageCreateDto dto);
        Task<ApiResponse<SliderImageResponseDto>> UpdateAsync(int id, SliderImageUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
