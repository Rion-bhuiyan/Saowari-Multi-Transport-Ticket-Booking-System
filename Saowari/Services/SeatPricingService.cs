using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.SeatPricing;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class SeatPricingService : ISeatPricingService
    {
        private readonly IRepository<SeatPricing> _repository;
        private readonly IMapper _mapper;
        private readonly SaowariDbContext _context;

        public SeatPricingService(IRepository<SeatPricing> repository, IMapper mapper, SaowariDbContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ApiResponse<IEnumerable<SeatPricingResponseDto>>> GetAllAsync()
        {
            var entities = await _context.SeatPricings
                .Include(sp => sp.SeatClass)
                .ToListAsync();
            var dtos = _mapper.Map<IEnumerable<SeatPricingResponseDto>>(entities);
            return ApiResponse<IEnumerable<SeatPricingResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<SeatPricingResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _context.SeatPricings
                .Include(sp => sp.SeatClass)
                .FirstOrDefaultAsync(sp => sp.PricingID == id);
            if (entity == null) return ApiResponse<SeatPricingResponseDto>.Fail("Not found");
            return ApiResponse<SeatPricingResponseDto>.Ok(_mapper.Map<SeatPricingResponseDto>(entity));
        }

        public async Task<ApiResponse<SeatPricingResponseDto>> CreateAsync(SeatPricingCreateDto dto)
        {
            var entity = _mapper.Map<SeatPricing>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();
            return ApiResponse<SeatPricingResponseDto>.Ok(_mapper.Map<SeatPricingResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<SeatPricingResponseDto>> UpdateAsync(int id, SeatPricingUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<SeatPricingResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<SeatPricingResponseDto>.Ok(_mapper.Map<SeatPricingResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }

        public async Task<ApiResponse<IEnumerable<SeatPricingResponseDto>>> GetByVehicleIdAsync(int vehicleId)
        {
            var entities = await _context.SeatPricings
                .Include(sp => sp.SeatClass)
                .Where(sp => sp.VehicleId == vehicleId)
                .ToListAsync();
            var dtos = _mapper.Map<IEnumerable<SeatPricingResponseDto>>(entities);
            return ApiResponse<IEnumerable<SeatPricingResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<bool>> BulkUpsertForVehicleAsync(int vehicleId, List<SeatClassPricingInputDto> pricings)
        {
            var existing = await _context.SeatPricings
                .Where(sp => sp.VehicleId == vehicleId)
                .ToListAsync();

            _context.SeatPricings.RemoveRange(existing);

            if (pricings != null)
            {
                foreach (var p in pricings)
                {
                    _context.SeatPricings.Add(new SeatPricing
                    {
                        VehicleId = vehicleId,
                        SeatClassId = p.SeatClassId,
                        Price = p.Price,
                        LastUpdate = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Vehicle seat pricing updated successfully");
        }
    }
}