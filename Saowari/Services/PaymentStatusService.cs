using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.PaymentStatus;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class PaymentStatusService : IPaymentStatusService
    {
        private readonly IRepository<PaymentStatus> _repository;
        private readonly IMapper _mapper;

        public PaymentStatusService(IRepository<PaymentStatus> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<PaymentStatusResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<PaymentStatusResponseDto>>(entities);
            return ApiResponse<IEnumerable<PaymentStatusResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<PaymentStatusResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<PaymentStatusResponseDto>.Fail("Not found");
            return ApiResponse<PaymentStatusResponseDto>.Ok(_mapper.Map<PaymentStatusResponseDto>(entity));
        }

        public async Task<ApiResponse<PaymentStatusResponseDto>> CreateAsync(PaymentStatusCreateDto dto)
        {
            var entity = _mapper.Map<PaymentStatus>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<PaymentStatusResponseDto>.Ok(_mapper.Map<PaymentStatusResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<PaymentStatusResponseDto>> UpdateAsync(int id, PaymentStatusUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<PaymentStatusResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<PaymentStatusResponseDto>.Ok(_mapper.Map<PaymentStatusResponseDto>(entity), "Updated successfully");
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