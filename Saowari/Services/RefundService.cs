using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Refund;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class RefundService : IRefundService
    {
        private readonly IRepository<Refund> _repository;
        private readonly IMapper _mapper;

        public RefundService(IRepository<Refund> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<RefundResponseDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync(includeProperties: "Booking,Booking.User,Payment,Payment.PaymentMethod,RefundStatus,RefundPolicy,UpdatedByUser");
            var dtos = _mapper.Map<IEnumerable<RefundResponseDto>>(entities);
            return ApiResponse<IEnumerable<RefundResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<RefundResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetFirstOrDefaultAsync(
                filter: r => r.RefundID == id,
                includeProperties: "Booking,Booking.User,Payment,Payment.PaymentMethod,RefundStatus,RefundPolicy,UpdatedByUser"
            );
            if (entity == null) return ApiResponse<RefundResponseDto>.Fail("Not found");
            return ApiResponse<RefundResponseDto>.Ok(_mapper.Map<RefundResponseDto>(entity));
        }

        public async Task<ApiResponse<RefundResponseDto>> CreateAsync(RefundCreateDto dto)
        {
            var entity = _mapper.Map<Refund>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<RefundResponseDto>.Ok(_mapper.Map<RefundResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<RefundResponseDto>> UpdateAsync(int id, RefundUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<RefundResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<RefundResponseDto>.Ok(_mapper.Map<RefundResponseDto>(entity), "Updated successfully");
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