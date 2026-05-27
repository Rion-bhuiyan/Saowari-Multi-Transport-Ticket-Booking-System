using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Vehicle;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IRepository<Vehicle> _repository;
        private readonly IMapper _mapper;
        private readonly Saowari.Data.SaowariDbContext _context;
        private readonly INotificationService _notificationService;

        public VehicleService(IRepository<Vehicle> repository, IMapper mapper, Saowari.Data.SaowariDbContext context, INotificationService notificationService)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<IEnumerable<VehicleResponseDto>>> GetAllAsync()
        {
            var entities = await _context.Vehicles
                .Include(v => v.SeatPricings)
                    .ThenInclude(sp => sp.SeatClass)
                .Include(v => v.Seats)
                .ToListAsync();
            var dtos = _mapper.Map<IEnumerable<VehicleResponseDto>>(entities);
            return ApiResponse<IEnumerable<VehicleResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<VehicleResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _context.Vehicles
                .Include(v => v.SeatPricings)
                    .ThenInclude(sp => sp.SeatClass)
                .Include(v => v.Seats)
                .FirstOrDefaultAsync(v => v.VehicleID == id);
            if (entity == null) return ApiResponse<VehicleResponseDto>.Fail("Not found");
            return ApiResponse<VehicleResponseDto>.Ok(_mapper.Map<VehicleResponseDto>(entity));
        }

        public async Task<ApiResponse<VehicleResponseDto>> CreateAsync(VehicleCreateDto dto)
        {
            var entity = _mapper.Map<Vehicle>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();

            // Set seat class pricings
            if (dto.SeatClassPricings != null && dto.SeatClassPricings.Any())
            {
                foreach (var p in dto.SeatClassPricings)
                {
                    _context.SeatPricings.Add(new SeatPricing
                    {
                        VehicleId = entity.VehicleID,
                        SeatClassId = p.SeatClassId,
                        Price = p.Price,
                        LastUpdate = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            if (dto.VisualLayout != null)
            {
                await GenerateSeatsAsync(entity.VehicleID, dto.VisualLayout);
            }

            // Reload to get seat class pricings populated correctly in response
            entity = await _context.Vehicles
                .Include(v => v.SeatPricings)
                    .ThenInclude(sp => sp.SeatClass)
                .Include(v => v.Seats)
                .FirstOrDefaultAsync(v => v.VehicleID == entity.VehicleID);

            try { await _notificationService.NotifyVehicleChangedAsync(entity, "Created"); } catch { }
            return ApiResponse<VehicleResponseDto>.Ok(_mapper.Map<VehicleResponseDto>(entity), "Created successfully");
        }

        public async Task<ApiResponse<VehicleResponseDto>> UpdateAsync(int id, VehicleUpdateDto dto)
        {
            var entity = await _context.Vehicles
                .Include(v => v.SeatPricings)
                .Include(v => v.Seats)
                .FirstOrDefaultAsync(v => v.VehicleID == id);
            if (entity == null) return ApiResponse<VehicleResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();

            // Update seat class pricings (remove existing, write new ones)
            _context.SeatPricings.RemoveRange(entity.SeatPricings);
            if (dto.SeatClassPricings != null && dto.SeatClassPricings.Any())
            {
                foreach (var p in dto.SeatClassPricings)
                {
                    _context.SeatPricings.Add(new SeatPricing
                    {
                        VehicleId = id,
                        SeatClassId = p.SeatClassId,
                        Price = p.Price,
                        LastUpdate = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }
            
            bool layoutChanged = false;
            if (dto.VisualLayout != null)
            {
                try 
                {
                    var oldLayout = entity.SeatLayoutConfig;
                    var newLayout = System.Text.Json.JsonSerializer.Serialize(dto.VisualLayout);
                    if (oldLayout != newLayout)
                    {
                        layoutChanged = true;
                    }
                } 
                catch { }
            }

            if (layoutChanged || !entity.Seats.Any())
            {
                try 
                {
                    if (dto.VisualLayout != null)
                    {
                        await GenerateSeatsAsync(entity.VehicleID, dto.VisualLayout);
                    }
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    return ApiResponse<VehicleResponseDto>.Fail("Vehicle saved, but cannot change seat layout because some seats are already booked. Please create a new vehicle for a different layout.");
                }
            }

            // Reload vehicle to get all pricing info
            entity = await _context.Vehicles
                .Include(v => v.SeatPricings)
                    .ThenInclude(sp => sp.SeatClass)
                .Include(v => v.Seats)
                .FirstOrDefaultAsync(v => v.VehicleID == id);

            try { await _notificationService.NotifyVehicleChangedAsync(entity, "Updated"); } catch { }
            return ApiResponse<VehicleResponseDto>.Ok(_mapper.Map<VehicleResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            try { await _notificationService.NotifyVehicleChangedAsync(entity, "Deleted"); } catch { }
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }

        public async Task<ApiResponse<bool>> GenerateSeatsAsync(int vehicleId, SeatLayoutConfigDto config)
        {
            var vehicle = await _context.Vehicles.Include(v => v.Seats).FirstOrDefaultAsync(v => v.VehicleID == vehicleId);
            if (vehicle == null) return ApiResponse<bool>.Fail("Vehicle not found");

            var newSeatNumbers = new List<string>();
            foreach (var deck in config.Decks)
            {
                foreach (var s in deck.Seats)
                {
                    if (!string.IsNullOrEmpty(s.SeatNumber))
                        newSeatNumbers.Add(s.SeatNumber.Trim().ToUpper());
                }
            }

            var seatsToRemove = vehicle.Seats.Where(s => !newSeatNumbers.Contains(s.SeatNumber)).ToList();
            if (seatsToRemove.Any())
            {
                _context.Seats.RemoveRange(seatsToRemove);
            }

            var defaultSeatClass = await _context.SeatClasses.FirstOrDefaultAsync();
            int defClassId = defaultSeatClass?.SeatClassId ?? 1;

            foreach (var deck in config.Decks)
            {
                foreach (var seatConfig in deck.Seats)
                {
                    if (string.IsNullOrEmpty(seatConfig.SeatNumber)) continue;
                    
                    var existingSeat = vehicle.Seats.FirstOrDefault(s => s.SeatNumber.Equals(seatConfig.SeatNumber.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (existingSeat != null)
                    {
                        existingSeat.SeatClassId = seatConfig.SeatClassId > 0 ? seatConfig.SeatClassId : defClassId;
                    }
                    else
                    {
                        _context.Seats.Add(new Seat
                        {
                            VehicleId = vehicleId,
                            SeatNumber = seatConfig.SeatNumber.Trim().ToUpper(),
                            SeatPriceing = 0,
                            SeatClassId = seatConfig.SeatClassId > 0 ? seatConfig.SeatClassId : defClassId
                        });
                    }
                }
            }

            vehicle.SeatLayoutConfig = System.Text.Json.JsonSerializer.Serialize(config);
            vehicle.TotalSeats = newSeatNumbers.Count;
            
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Visual layout saved successfully");
        }

        public async Task<ApiResponse<bool>> UpdateSeatClassesAsync(int vehicleId, List<SeatClassAssignmentDto> assignments)
        {
            var seats = await _context.Seats.Where(s => s.VehicleId == vehicleId).ToListAsync();
            if (seats == null || !seats.Any())
            {
                return ApiResponse<bool>.Fail("No seats found for this vehicle. Please generate seats first.");
            }

            foreach (var assignment in assignments)
            {
                var seat = seats.FirstOrDefault(s => s.SeatNumber.Equals(assignment.SeatNumber, StringComparison.OrdinalIgnoreCase));
                if (seat != null)
                {
                    seat.SeatClassId = assignment.SeatClassId;
                }
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Seat classes updated successfully.");
        }
    }
}