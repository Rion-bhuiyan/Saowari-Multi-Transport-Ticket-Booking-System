using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.RefundStatus;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class RefundStatusService : IRefundStatusService
    {
        private readonly IRepository<RefundStatus> _repository;
        private readonly IMapper _mapper;

        public RefundStatusService(IRepository<RefundStatus> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<RefundStatusResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<RefundStatusResponseDto>>(entities);
            return ApiResponse<IEnumerable<RefundStatusResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<RefundStatusResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<RefundStatusResponseDto>.Fail("Not found");
            return ApiResponse<RefundStatusResponseDto>.Ok(_mapper.Map<RefundStatusResponseDto>(entity));
        }

        public async Task<ApiResponse<RefundStatusResponseDto>> CreateAsync(RefundStatusCreateDto dto)
        {
            var entity = _mapper.Map<RefundStatus>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<RefundStatusResponseDto>.Ok(_mapper.Map<RefundStatusResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<RefundStatusResponseDto>> UpdateAsync(int id, RefundStatusUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<RefundStatusResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<RefundStatusResponseDto>.Ok(_mapper.Map<RefundStatusResponseDto>(entity), "Updated successfully");
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