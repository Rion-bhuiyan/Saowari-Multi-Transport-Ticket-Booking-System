using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.PaymentCancel;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class PaymentCancelService : IPaymentCancelService
    {
        private readonly IRepository<PaymentCancel> _repository;
        private readonly IMapper _mapper;

        public PaymentCancelService(IRepository<PaymentCancel> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<PaymentCancelResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<PaymentCancelResponseDto>>(entities);
            return ApiResponse<IEnumerable<PaymentCancelResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<PaymentCancelResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<PaymentCancelResponseDto>.Fail("Not found");
            return ApiResponse<PaymentCancelResponseDto>.Ok(_mapper.Map<PaymentCancelResponseDto>(entity));
        }

        public async Task<ApiResponse<PaymentCancelResponseDto>> CreateAsync(PaymentCancelCreateDto dto)
        {
            var entity = _mapper.Map<PaymentCancel>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<PaymentCancelResponseDto>.Ok(_mapper.Map<PaymentCancelResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<PaymentCancelResponseDto>> UpdateAsync(int id, PaymentCancelUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<PaymentCancelResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<PaymentCancelResponseDto>.Ok(_mapper.Map<PaymentCancelResponseDto>(entity), "Updated successfully");
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