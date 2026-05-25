using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Location;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class LocationService : ILocationService
    {
        private readonly IRepository<Location> _repository;
        private readonly IMapper _mapper;

        public LocationService(IRepository<Location> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<LocationResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<LocationResponseDto>>(entities);
            return ApiResponse<IEnumerable<LocationResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<LocationResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<LocationResponseDto>.Fail("Not found");
            return ApiResponse<LocationResponseDto>.Ok(_mapper.Map<LocationResponseDto>(entity));
        }

        public async Task<ApiResponse<LocationResponseDto>> CreateAsync(LocationCreateDto dto)
        {
            var entity = _mapper.Map<Location>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<LocationResponseDto>.Ok(_mapper.Map<LocationResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<LocationResponseDto>> UpdateAsync(int id, LocationUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<LocationResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<LocationResponseDto>.Ok(_mapper.Map<LocationResponseDto>(entity), "Updated successfully");
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