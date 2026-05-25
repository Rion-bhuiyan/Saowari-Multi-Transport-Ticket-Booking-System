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

            var config = new SeatLayoutConfigDto 
            { 
                IsDoubleDecker = dto.IsDoubleDecker, 
                ContinuousBackRow = dto.ContinuousBackRow,
                LayoutPreset = dto.LayoutPreset
            };
            await GenerateSeatsAsync(entity.VehicleID, config);

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
            
            // Generate seats only if layout changes to preserve custom seat classes
            bool layoutChanged = true;
            if (!string.IsNullOrEmpty(entity.SeatLayoutConfig))
            {
                try 
                {
                    var parsed = System.Text.Json.JsonDocument.Parse(entity.SeatLayoutConfig);
                    bool oldIsDouble = parsed.RootElement.TryGetProperty("IsDoubleDecker", out var prop1) && prop1.ValueKind != System.Text.Json.JsonValueKind.Null && prop1.GetBoolean();
                    bool oldCont = parsed.RootElement.TryGetProperty("ContinuousBackRow", out var prop2) && prop2.ValueKind != System.Text.Json.JsonValueKind.Null && prop2.GetBoolean();
                    string oldPreset = parsed.RootElement.TryGetProperty("LayoutPreset", out var prop3) && prop3.ValueKind != System.Text.Json.JsonValueKind.Null ? prop3.GetString() : "standard";
                    
                    if (oldIsDouble == dto.IsDoubleDecker && 
                        oldCont == dto.ContinuousBackRow && 
                        (oldPreset ?? "") == (dto.LayoutPreset ?? ""))
                    {
                        layoutChanged = false;
                    }
                } 
                catch { }
            }

            if (layoutChanged || !entity.Seats.Any())
            {
                try 
                {
                    var config = new SeatLayoutConfigDto 
                    { 
                        IsDoubleDecker = dto.IsDoubleDecker, 
                        ContinuousBackRow = dto.ContinuousBackRow,
                        LayoutPreset = dto.LayoutPreset
                    };
                    await GenerateSeatsAsync(entity.VehicleID, config);
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
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return ApiResponse<bool>.Fail("Vehicle not found");

            var existingSeats = _context.Seats.Where(s => s.VehicleId == vehicleId);
            _context.Seats.RemoveRange(existingSeats);

            var defaultSeatClass = _context.SeatClasses.FirstOrDefault()?.SeatClassId ?? 1;
            var seats = new List<Seat>();
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int seatsPerRow = 3;
            int aisleAfterCol = 1;
            int backRowSeats = 4;

            if (!string.IsNullOrEmpty(config.LayoutPreset))
            {
                switch (config.LayoutPreset.ToLower())
                {
                    case "economy":
                        seatsPerRow = 4;
                        aisleAfterCol = 2;
                        backRowSeats = 5;
                        break;
                    case "sleeper":
                        seatsPerRow = 4;
                        aisleAfterCol = 2;
                        backRowSeats = 5;
                        break;
                    case "minibus":
                        seatsPerRow = 2;
                        aisleAfterCol = 1;
                        backRowSeats = 3;
                        break;
                    default:
                        seatsPerRow = 3;
                        aisleAfterCol = 1;
                        backRowSeats = 4;
                        break;
                }
            }

            int decks = config.IsDoubleDecker ? 2 : 1;
            int totalCapacity = vehicle.TotalSeats;
            int generatedForDeck = 0;

            for (int d = 1; d <= decks; d++)
            {
                int capacityForThisDeck = d == 1 ? (int)Math.Ceiling(totalCapacity / (double)decks) : totalCapacity / decks;
                
                int numRegularRows;
                if (config.ContinuousBackRow)
                {
                    numRegularRows = (int)Math.Ceiling((double)Math.Max(0, capacityForThisDeck - backRowSeats) / seatsPerRow);
                }
                else
                {
                    numRegularRows = (int)Math.Ceiling((double)capacityForThisDeck / seatsPerRow);
                }

                string deckPrefix = config.IsDoubleDecker ? (d == 1 ? "L" : "U") : "";
                generatedForDeck = 0;

                for (int r = 0; r < numRegularRows; r++)
                {
                    string rowLetter = r < alphabet.Length ? alphabet[r].ToString() : $"R{r + 1}";
                    for (int c = 1; c <= seatsPerRow; c++)
                    {
                        if (generatedForDeck >= capacityForThisDeck) break;

                        seats.Add(new Seat
                        {
                            VehicleId = vehicleId,
                            SeatNumber = $"{deckPrefix}{rowLetter}{c}",
                            SeatPriceing = 0,
                            SeatClassId = defaultSeatClass
                        });
                        generatedForDeck++;
                    }
                }

                if (config.ContinuousBackRow)
                {
                    string backLetter = numRegularRows < alphabet.Length
                        ? alphabet[numRegularRows].ToString()
                        : $"R{numRegularRows + 1}";
                    for (int c = 1; c <= backRowSeats; c++)
                    {
                        if (generatedForDeck >= capacityForThisDeck) break;

                        seats.Add(new Seat
                        {
                            VehicleId = vehicleId,
                            SeatNumber = $"{deckPrefix}{backLetter}{c}",
                            SeatPriceing = 0,
                            SeatClassId = defaultSeatClass
                        });
                        generatedForDeck++;
                    }
                }
            }

            // Calculate max rows for the config JSON by taking max of both decks
            int maxCapacityAnyDeck = (int)Math.Ceiling(totalCapacity / (double)decks);
            int maxRegularRows = config.ContinuousBackRow 
                ? (int)Math.Ceiling((double)Math.Max(0, maxCapacityAnyDeck - backRowSeats) / seatsPerRow)
                : (int)Math.Ceiling((double)maxCapacityAnyDeck / seatsPerRow);

            int totalRows = maxRegularRows + (config.ContinuousBackRow ? 1 : 0);
            
            vehicle.SeatLayoutConfig = System.Text.Json.JsonSerializer.Serialize(new
            {
                Rows = totalRows,
                Columns = seatsPerRow,
                AisleAfterColumn = aisleAfterCol,
                config.IsDoubleDecker,
                config.ContinuousBackRow,
                LayoutPreset = config.LayoutPreset ?? "standard"
            });
            vehicle.TotalSeats = seats.Count;

            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, $"{seats.Count} seats generated successfully");
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