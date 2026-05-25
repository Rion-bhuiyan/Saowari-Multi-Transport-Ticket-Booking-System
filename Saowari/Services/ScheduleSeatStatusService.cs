using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.ScheduleSeatStatus;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class ScheduleSeatStatusService : IScheduleSeatStatusService
    {
        private readonly IRepository<ScheduleSeatStatus> _repository;
        private readonly IMapper _mapper;

        public ScheduleSeatStatusService(IRepository<ScheduleSeatStatus> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<ScheduleSeatStatusResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ScheduleSeatStatusResponseDto>>(entities);
            return ApiResponse<IEnumerable<ScheduleSeatStatusResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<ScheduleSeatStatusResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<ScheduleSeatStatusResponseDto>.Fail("Not found");
            return ApiResponse<ScheduleSeatStatusResponseDto>.Ok(_mapper.Map<ScheduleSeatStatusResponseDto>(entity));
        }

        public async Task<ApiResponse<ScheduleSeatStatusResponseDto>> CreateAsync(ScheduleSeatStatusCreateDto dto)
        {
            var entity = _mapper.Map<ScheduleSeatStatus>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<ScheduleSeatStatusResponseDto>.Ok(_mapper.Map<ScheduleSeatStatusResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<ScheduleSeatStatusResponseDto>> UpdateAsync(int id, ScheduleSeatStatusUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<ScheduleSeatStatusResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<ScheduleSeatStatusResponseDto>.Ok(_mapper.Map<ScheduleSeatStatusResponseDto>(entity), "Updated successfully");
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