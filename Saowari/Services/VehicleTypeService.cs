using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.VehicleType;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class VehicleTypeService : IVehicleTypeService
    {
        private readonly IRepository<VehicleType> _repository;
        private readonly IMapper _mapper;

        public VehicleTypeService(IRepository<VehicleType> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<VehicleTypeResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<VehicleTypeResponseDto>>(entities);
            return ApiResponse<IEnumerable<VehicleTypeResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<VehicleTypeResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<VehicleTypeResponseDto>.Fail("Not found");
            return ApiResponse<VehicleTypeResponseDto>.Ok(_mapper.Map<VehicleTypeResponseDto>(entity));
        }

        public async Task<ApiResponse<VehicleTypeResponseDto>> CreateAsync(VehicleTypeCreateDto dto)
        {
            var entity = _mapper.Map<VehicleType>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<VehicleTypeResponseDto>.Ok(_mapper.Map<VehicleTypeResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<VehicleTypeResponseDto>> UpdateAsync(int id, VehicleTypeUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<VehicleTypeResponseDto>.Fail("Not found");
            
            entity.VehicleTypeName = dto.VehicleTypeName;
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<VehicleTypeResponseDto>.Ok(_mapper.Map<VehicleTypeResponseDto>(entity), "Updated successfully");
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