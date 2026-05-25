using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.SeatClass;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class SeatClassService : ISeatClassService
    {
        private readonly IRepository<SeatClass> _repository;
        private readonly IMapper _mapper;

        public SeatClassService(IRepository<SeatClass> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<SeatClassResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SeatClassResponseDto>>(entities);
            return ApiResponse<IEnumerable<SeatClassResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<SeatClassResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SeatClassResponseDto>.Fail("Not found");
            return ApiResponse<SeatClassResponseDto>.Ok(_mapper.Map<SeatClassResponseDto>(entity));
        }

        public async Task<ApiResponse<SeatClassResponseDto>> CreateAsync(SeatClassCreateDto dto)
        {
            var entity = _mapper.Map<SeatClass>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<SeatClassResponseDto>.Ok(_mapper.Map<SeatClassResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<SeatClassResponseDto>> UpdateAsync(int id, SeatClassUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SeatClassResponseDto>.Fail("Not found");
            
            entity.SeatClassName = dto.SeatClassName;
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<SeatClassResponseDto>.Ok(_mapper.Map<SeatClassResponseDto>(entity), "Updated successfully");
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