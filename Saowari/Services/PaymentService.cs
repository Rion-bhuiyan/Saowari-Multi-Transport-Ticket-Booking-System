using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Payment;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Saowari.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _repository;
        private readonly IMapper _mapper;
        private readonly Saowari.Data.SaowariDbContext _context;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public PaymentService(IRepository<Payment> repository, IMapper mapper, Saowari.Data.SaowariDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<IEnumerable<PaymentResponseDto>>> GetAllAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userRole = user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var companyIdStr = user?.FindFirst("CompanyId")?.Value;

            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Schedule)
                        .ThenInclude(s => s.Vehicle)
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .AsQueryable();

            if (userRole == "CompanyManager" && int.TryParse(companyIdStr, out int companyId))
            {
                query = query.Where(p => p.Booking != null && p.Booking.Schedule != null && p.Booking.Schedule.Vehicle != null && p.Booking.Schedule.Vehicle.CompanyId == companyId);
            }

            var entities = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query.OrderByDescending(p => p.CreatedAt));
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