using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.SeatStatus;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class SeatStatusService : ISeatStatusService
    {
        private readonly IRepository<SeatStatus> _repository;
        private readonly IMapper _mapper;

        public SeatStatusService(IRepository<SeatStatus> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<SeatStatusResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SeatStatusResponseDto>>(entities);
            return ApiResponse<IEnumerable<SeatStatusResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<SeatStatusResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SeatStatusResponseDto>.Fail("Not found");
            return ApiResponse<SeatStatusResponseDto>.Ok(_mapper.Map<SeatStatusResponseDto>(entity));
        }

        public async Task<ApiResponse<SeatStatusResponseDto>> CreateAsync(SeatStatusCreateDto dto)
        {
            var entity = _mapper.Map<SeatStatus>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<SeatStatusResponseDto>.Ok(_mapper.Map<SeatStatusResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<SeatStatusResponseDto>> UpdateAsync(int id, SeatStatusUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SeatStatusResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<SeatStatusResponseDto>.Ok(_mapper.Map<SeatStatusResponseDto>(entity), "Updated successfully");
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