using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Supervisor;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class SupervisorService : ISupervisorService
    {
        private readonly IRepository<Supervisor> _repository;
        private readonly IMapper _mapper;

        public SupervisorService(IRepository<Supervisor> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<SupervisorResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SupervisorResponseDto>>(entities);
            return ApiResponse<IEnumerable<SupervisorResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<SupervisorResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SupervisorResponseDto>.Fail("Not found");
            return ApiResponse<SupervisorResponseDto>.Ok(_mapper.Map<SupervisorResponseDto>(entity));
        }

        public async Task<ApiResponse<SupervisorResponseDto>> CreateAsync(SupervisorCreateDto dto)
        {
            var entity = _mapper.Map<Supervisor>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<SupervisorResponseDto>.Ok(_mapper.Map<SupervisorResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<SupervisorResponseDto>> UpdateAsync(int id, SupervisorUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SupervisorResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<SupervisorResponseDto>.Ok(_mapper.Map<SupervisorResponseDto>(entity), "Updated successfully");
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