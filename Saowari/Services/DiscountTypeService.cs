using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.DiscountType;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class DiscountTypeService : IDiscountTypeService
    {
        private readonly IRepository<DiscountType> _repository;
        private readonly IMapper _mapper;

        public DiscountTypeService(IRepository<DiscountType> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<DiscountTypeResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<DiscountTypeResponseDto>>(entities);
            return ApiResponse<IEnumerable<DiscountTypeResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<DiscountTypeResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<DiscountTypeResponseDto>.Fail("Not found");
            return ApiResponse<DiscountTypeResponseDto>.Ok(_mapper.Map<DiscountTypeResponseDto>(entity));
        }

        public async Task<ApiResponse<DiscountTypeResponseDto>> CreateAsync(DiscountTypeCreateDto dto)
        {
            var entity = _mapper.Map<DiscountType>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<DiscountTypeResponseDto>.Ok(_mapper.Map<DiscountTypeResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<DiscountTypeResponseDto>> UpdateAsync(int id, DiscountTypeUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<DiscountTypeResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<DiscountTypeResponseDto>.Ok(_mapper.Map<DiscountTypeResponseDto>(entity), "Updated successfully");
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