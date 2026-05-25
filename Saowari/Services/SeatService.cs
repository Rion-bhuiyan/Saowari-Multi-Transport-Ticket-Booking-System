using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Seat;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class SeatService : ISeatService
    {
        private readonly IRepository<Seat> _repository;
        private readonly IMapper _mapper;

        public SeatService(IRepository<Seat> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<SeatResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SeatResponseDto>>(entities);
            return ApiResponse<IEnumerable<SeatResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<SeatResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SeatResponseDto>.Fail("Not found");
            return ApiResponse<SeatResponseDto>.Ok(_mapper.Map<SeatResponseDto>(entity));
        }

        public async Task<ApiResponse<SeatResponseDto>> CreateAsync(SeatCreateDto dto)
        {
            var entity = _mapper.Map<Seat>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<SeatResponseDto>.Ok(_mapper.Map<SeatResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<SeatResponseDto>> UpdateAsync(int id, SeatUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SeatResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<SeatResponseDto>.Ok(_mapper.Map<SeatResponseDto>(entity), "Updated successfully");
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