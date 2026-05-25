using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.DriverInformtion;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class DriverInformtionService : IDriverInformtionService
    {
        private readonly IRepository<DriverInformtion> _repository;
        private readonly IMapper _mapper;

        public DriverInformtionService(IRepository<DriverInformtion> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<DriverInformtionResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<DriverInformtionResponseDto>>(entities);
            return ApiResponse<IEnumerable<DriverInformtionResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<DriverInformtionResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<DriverInformtionResponseDto>.Fail("Not found");
            return ApiResponse<DriverInformtionResponseDto>.Ok(_mapper.Map<DriverInformtionResponseDto>(entity));
        }

        public async Task<ApiResponse<DriverInformtionResponseDto>> CreateAsync(DriverInformtionCreateDto dto)
        {
            var entity = _mapper.Map<DriverInformtion>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<DriverInformtionResponseDto>.Ok(_mapper.Map<DriverInformtionResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<DriverInformtionResponseDto>> UpdateAsync(int id, DriverInformtionUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<DriverInformtionResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<DriverInformtionResponseDto>.Ok(_mapper.Map<DriverInformtionResponseDto>(entity), "Updated successfully");
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