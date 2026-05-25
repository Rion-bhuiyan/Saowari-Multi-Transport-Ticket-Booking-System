using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.CompanyType;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class CompanyTypeService : ICompanyTypeService
    {
        private readonly IRepository<CompanyType> _repository;
        private readonly IMapper _mapper;

        public CompanyTypeService(IRepository<CompanyType> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<CompanyTypeResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CompanyTypeResponseDto>>(entities);
            return ApiResponse<IEnumerable<CompanyTypeResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<CompanyTypeResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<CompanyTypeResponseDto>.Fail("Not found");
            return ApiResponse<CompanyTypeResponseDto>.Ok(_mapper.Map<CompanyTypeResponseDto>(entity));
        }

        public async Task<ApiResponse<CompanyTypeResponseDto>> CreateAsync(CompanyTypeCreateDto dto)
        {
            var entity = _mapper.Map<CompanyType>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<CompanyTypeResponseDto>.Ok(_mapper.Map<CompanyTypeResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<CompanyTypeResponseDto>> UpdateAsync(int id, CompanyTypeUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<CompanyTypeResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<CompanyTypeResponseDto>.Ok(_mapper.Map<CompanyTypeResponseDto>(entity), "Updated successfully");
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