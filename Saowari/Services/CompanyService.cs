using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Company;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _repository;
        private readonly IMapper _mapper;

        public CompanyService(IRepository<Company> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<CompanyResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CompanyResponseDto>>(entities);
            return ApiResponse<IEnumerable<CompanyResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<CompanyResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<CompanyResponseDto>.Fail("Not found");
            return ApiResponse<CompanyResponseDto>.Ok(_mapper.Map<CompanyResponseDto>(entity));
        }

        public async Task<ApiResponse<CompanyResponseDto>> CreateAsync(CompanyCreateDto dto)
        {
            var entity = _mapper.Map<Company>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<CompanyResponseDto>.Ok(_mapper.Map<CompanyResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<CompanyResponseDto>> UpdateAsync(int id, CompanyUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<CompanyResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<CompanyResponseDto>.Ok(_mapper.Map<CompanyResponseDto>(entity), "Updated successfully");
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