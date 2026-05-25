using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Payment;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _repository;
        private readonly IMapper _mapper;

        public PaymentService(IRepository<Payment> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<PaymentResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<PaymentResponseDto>>(entities);
            return ApiResponse<IEnumerable<PaymentResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<PaymentResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<PaymentResponseDto>.Fail("Not found");
            return ApiResponse<PaymentResponseDto>.Ok(_mapper.Map<PaymentResponseDto>(entity));
        }

        public async Task<ApiResponse<PaymentResponseDto>> CreateAsync(PaymentCreateDto dto)
        {
            var entity = _mapper.Map<Payment>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<PaymentResponseDto>.Ok(_mapper.Map<PaymentResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<PaymentResponseDto>> UpdateAsync(int id, PaymentUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<PaymentResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<PaymentResponseDto>.Ok(_mapper.Map<PaymentResponseDto>(entity), "Updated successfully");
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