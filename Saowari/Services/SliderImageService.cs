using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.SliderImage;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class SliderImageService : ISliderImageService
    {
        private readonly IRepository<SliderImage> _repository;
        private readonly IMapper _mapper;

        public SliderImageService(IRepository<SliderImage> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<SliderImageResponseDto>>> GetAllActiveAsync()
        {
            var entities = await _repository.GetAllAsync();
            var activeEntities = entities
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList();
            
            var dtos = _mapper.Map<IEnumerable<SliderImageResponseDto>>(activeEntities);
            return ApiResponse<IEnumerable<SliderImageResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<IEnumerable<SliderImageResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var orderedEntities = entities
                .OrderBy(x => x.DisplayOrder)
                .ToList();
            
            var dtos = _mapper.Map<IEnumerable<SliderImageResponseDto>>(orderedEntities);
            return ApiResponse<IEnumerable<SliderImageResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<SliderImageResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SliderImageResponseDto>.Fail("Not found");
            return ApiResponse<SliderImageResponseDto>.Ok(_mapper.Map<SliderImageResponseDto>(entity));
        }

        public async Task<ApiResponse<SliderImageResponseDto>> CreateAsync(SliderImageCreateDto dto)
        {
            var entity = _mapper.Map<SliderImage>(dto);
            // ImageUrl will be set by the Controller if a file is uploaded
            if (!string.IsNullOrEmpty(dto.ImageUrl))
            {
                entity.ImageUrl = dto.ImageUrl;
            }
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<SliderImageResponseDto>.Ok(_mapper.Map<SliderImageResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<SliderImageResponseDto>> UpdateAsync(int id, SliderImageUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SliderImageResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            if (!string.IsNullOrEmpty(dto.ImageUrl))
            {
                entity.ImageUrl = dto.ImageUrl;
            }
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<SliderImageResponseDto>.Ok(_mapper.Map<SliderImageResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }
    }
}
