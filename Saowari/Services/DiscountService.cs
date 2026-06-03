using AutoMapper;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Discount;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using System.Linq;

namespace Saowari.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IRepository<Discount> _repository;
        private readonly IMapper _mapper;
        private readonly SaowariDbContext _context;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public DiscountService(IRepository<Discount> repository, IMapper mapper, SaowariDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<IEnumerable<DiscountResponseDto>>> GetAllAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userRole = user?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var companyIdStr = user?.FindFirst("CompanyId")?.Value;

            var query = _context.Discounts
                .Include(d => d.DiscountType)
                .Include(d => d.Company)
                .Include(d => d.Route)
                    .ThenInclude(r => r.FromLocation)
                .Include(d => d.Route)
                    .ThenInclude(r => r.ToLocation)
                .Include(d => d.VehicleType)
                .AsQueryable();

            if (userRole == "CompanyManager" && int.TryParse(companyIdStr, out int companyId))
            {
                query = query.Where(d => d.CompanyId == companyId);
            }

            var entities = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<DiscountResponseDto>>(entities);
            return ApiResponse<IEnumerable<DiscountResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<DiscountResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<DiscountResponseDto>.Fail("Not found");
            return ApiResponse<DiscountResponseDto>.Ok(_mapper.Map<DiscountResponseDto>(entity));
        }

        public async Task<ApiResponse<DiscountResponseDto>> CreateAsync(DiscountCreateDto dto)
        {
            var entity = _mapper.Map<Discount>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<DiscountResponseDto>.Ok(_mapper.Map<DiscountResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<DiscountResponseDto>> UpdateAsync(int id, DiscountUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<DiscountResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<DiscountResponseDto>.Ok(_mapper.Map<DiscountResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            // Validation: Cannot delete if it is being used in any booking
            bool isUsed = await _context.Bookings.AnyAsync(b => b.DiscountID == id);
            if (isUsed)
            {
                return ApiResponse<bool>.Fail("Cannot delete this discount because it has already been used in one or more bookings. Consider setting it to inactive instead.");
            }
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }

        public async Task<ApiResponse<CouponValidationResponseDto>> ValidateCouponAsync(CouponValidationRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CouponCode))
            {
                return ApiResponse<CouponValidationResponseDto>.Ok(new CouponValidationResponseDto { IsValid = false, Message = "Coupon code is empty." });
            }

            var schedule = await _context.Schedules.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.ScheduleID == request.ScheduleId);
            if (schedule == null)
            {
                return ApiResponse<CouponValidationResponseDto>.Fail("Schedule not found.");
            }

            var companyId = schedule.Vehicle?.CompanyId ?? 0;
            var routeId = schedule.RouteId;
            var vehicleTypeId = schedule.Vehicle?.VehicleTypeId ?? 0;

            var coupon = await _context.Discounts.Include(d => d.DiscountType).FirstOrDefaultAsync(d => d.CouponCode == request.CouponCode && d.IsActive && d.CompanyId == companyId);
            
            if (coupon == null)
            {
                return ApiResponse<CouponValidationResponseDto>.Ok(new CouponValidationResponseDto { IsValid = false, Message = "Invalid coupon code." });
            }

            var now = System.DateTime.UtcNow;
            if (coupon.StartDate > now || coupon.EndDate < now)
            {
                return ApiResponse<CouponValidationResponseDto>.Ok(new CouponValidationResponseDto { IsValid = false, Message = "This coupon has expired or is not active yet." });
            }

            if (coupon.MinTicketAmount.HasValue && request.TotalTicketAmount < coupon.MinTicketAmount.Value)
            {
                return ApiResponse<CouponValidationResponseDto>.Ok(new CouponValidationResponseDto { IsValid = false, Message = $"Minimum ticket amount of {coupon.MinTicketAmount.Value} required." });
            }

            if (coupon.RouteId.HasValue && coupon.RouteId != routeId)
            {
                return ApiResponse<CouponValidationResponseDto>.Ok(new CouponValidationResponseDto { IsValid = false, Message = "This coupon is not valid for this route." });
            }

            if (coupon.VehicleTypeId.HasValue && coupon.VehicleTypeId != vehicleTypeId)
            {
                return ApiResponse<CouponValidationResponseDto>.Ok(new CouponValidationResponseDto { IsValid = false, Message = "This coupon is not valid for this vehicle type." });
            }

            decimal discountAmount = 0;
            bool isFlatAmount = coupon.DiscountType != null && coupon.DiscountType.DiscountTypeName.Contains("Flat", System.StringComparison.OrdinalIgnoreCase);
            bool isPercentage = coupon.DiscountType != null && coupon.DiscountType.DiscountTypeName.Contains("Percent", System.StringComparison.OrdinalIgnoreCase);

            if (isPercentage)
            {
                discountAmount = (request.TotalTicketAmount * coupon.DiscountValue) / 100m;
            }
            else if (isFlatAmount)
            {
                discountAmount = coupon.DiscountValue;
            }
            else
            {
                // Fallback to reversed logic if string check fails
                if (coupon.DiscountTypeId == 2)
                {
                    discountAmount = (request.TotalTicketAmount * coupon.DiscountValue) / 100m;
                }
                else
                {
                    discountAmount = coupon.DiscountValue;
                }
            }

            // Cap the discount amount to the total ticket amount
            if (discountAmount > request.TotalTicketAmount)
            {
                discountAmount = request.TotalTicketAmount;
            }

            return ApiResponse<CouponValidationResponseDto>.Ok(new CouponValidationResponseDto 
            { 
                IsValid = true, 
                Message = "Coupon applied successfully!", 
                DiscountAmount = discountAmount,
                DiscountId = coupon.DiscountID,
                DiscountValue = coupon.DiscountValue,
                IsPercentage = isPercentage || coupon.DiscountTypeId == 2
            });
        }
    }
}