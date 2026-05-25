using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.PaymentMethod;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly SaowariDbContext _context;
        private readonly IMapper _mapper;

        public PaymentMethodService(SaowariDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<PaymentMethodResponseDto>>> GetAllAsync()
        {
            var entities = await _context.PaymentMethods.ToListAsync();
            return ApiResponse<IEnumerable<PaymentMethodResponseDto>>.Ok(_mapper.Map<IEnumerable<PaymentMethodResponseDto>>(entities));
        }

        public async Task<ApiResponse<IEnumerable<PaymentMethodResponseDto>>> GetActiveAsync()
        {
            var entities = await _context.PaymentMethods.Where(m => m.IsActive).ToListAsync();
            return ApiResponse<IEnumerable<PaymentMethodResponseDto>>.Ok(_mapper.Map<IEnumerable<PaymentMethodResponseDto>>(entities));
        }

        public async Task<ApiResponse<PaymentMethodResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _context.PaymentMethods.FindAsync(id);
            if (entity == null) return ApiResponse<PaymentMethodResponseDto>.Fail("Payment method not found");
            return ApiResponse<PaymentMethodResponseDto>.Ok(_mapper.Map<PaymentMethodResponseDto>(entity));
        }

        public async Task<ApiResponse<PaymentMethodResponseDto>> CreateAsync(PaymentMethodCreateDto dto)
        {
            var entity = new PaymentMethod
            {
                PaymentMethodName = dto.PaymentMethodName,
                ProcessingFeePercent = dto.ProcessingFeePercent,
                VATPercent = dto.VATPercent,
                IsActive = dto.IsActive,
                LogoUrl = dto.LogoUrl
            };

            _context.PaymentMethods.Add(entity);
            await _context.SaveChangesAsync();
            return ApiResponse<PaymentMethodResponseDto>.Ok(_mapper.Map<PaymentMethodResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<PaymentMethodResponseDto>> UpdateAsync(int id, PaymentMethodUpdateDto dto)
        {
            var entity = await _context.PaymentMethods.FindAsync(id);
            if (entity == null) return ApiResponse<PaymentMethodResponseDto>.Fail("Payment method not found");

            entity.PaymentMethodName = dto.PaymentMethodName;
            entity.ProcessingFeePercent = dto.ProcessingFeePercent;
            entity.VATPercent = dto.VATPercent;
            entity.IsActive = dto.IsActive;

            // Only overwrite logo if a new one was uploaded
            if (!string.IsNullOrEmpty(dto.LogoUrl))
                entity.LogoUrl = dto.LogoUrl;

            _context.PaymentMethods.Update(entity);
            await _context.SaveChangesAsync();
            return ApiResponse<PaymentMethodResponseDto>.Ok(_mapper.Map<PaymentMethodResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _context.PaymentMethods.FindAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Payment method not found");

            // Check if any existing payments reference this method
            bool isReferenced = await _context.Payments.AnyAsync(p => p.PaymentMethodId == id);
            if (isReferenced)
            {
                return ApiResponse<bool>.Fail(
                    "This payment method cannot be deleted because it is already used in existing transactions. " +
                    "Please deactivate it instead so it is no longer available for new bookings."
                );
            }

            _context.PaymentMethods.Remove(entity);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }
    }
}