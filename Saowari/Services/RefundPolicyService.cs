using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.RefundPolicy;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class RefundPolicyService : IRefundPolicyService
    {
        private readonly IRepository<RefundPolicy> _repository;
        private readonly IMapper _mapper;

        public RefundPolicyService(IRepository<RefundPolicy> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<RefundPolicyResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<RefundPolicyResponseDto>>(entities);
            return ApiResponse<IEnumerable<RefundPolicyResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<RefundPolicyResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<RefundPolicyResponseDto>.Fail("Not found");
            return ApiResponse<RefundPolicyResponseDto>.Ok(_mapper.Map<RefundPolicyResponseDto>(entity));
        }

        public async Task<ApiResponse<RefundPolicyResponseDto>> CreateAsync(RefundPolicyCreateDto dto)
        {
            var entity = _mapper.Map<RefundPolicy>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<RefundPolicyResponseDto>.Ok(_mapper.Map<RefundPolicyResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<RefundPolicyResponseDto>> UpdateAsync(int id, RefundPolicyUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<RefundPolicyResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<RefundPolicyResponseDto>.Ok(_mapper.Map<RefundPolicyResponseDto>(entity), "Updated successfully");
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