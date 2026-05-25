using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.ScheduleStatus;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class ScheduleStatusService : IScheduleStatusService
    {
        private readonly IRepository<ScheduleStatus> _repository;
        private readonly IMapper _mapper;

        public ScheduleStatusService(IRepository<ScheduleStatus> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<ScheduleStatusResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ScheduleStatusResponseDto>>(entities);
            return ApiResponse<IEnumerable<ScheduleStatusResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<ScheduleStatusResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<ScheduleStatusResponseDto>.Fail("Not found");
            return ApiResponse<ScheduleStatusResponseDto>.Ok(_mapper.Map<ScheduleStatusResponseDto>(entity));
        }

        public async Task<ApiResponse<ScheduleStatusResponseDto>> CreateAsync(ScheduleStatusCreateDto dto)
        {
            var entity = _mapper.Map<ScheduleStatus>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<ScheduleStatusResponseDto>.Ok(_mapper.Map<ScheduleStatusResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<ScheduleStatusResponseDto>> UpdateAsync(int id, ScheduleStatusUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<ScheduleStatusResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<ScheduleStatusResponseDto>.Ok(_mapper.Map<ScheduleStatusResponseDto>(entity), "Updated successfully");
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