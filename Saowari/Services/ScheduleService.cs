using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Route;
using Saowari.Models.DTOs.Schedule;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IRepository<Schedule> _repository;
        private readonly IMapper _mapper;
        private readonly SaowariDbContext _db;
        private readonly INotificationService _notificationService;

        public ScheduleService(IRepository<Schedule> repository, IMapper mapper, SaowariDbContext db, INotificationService notificationService)
        {
            _repository = repository;
            _mapper = mapper;
            _db = db;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<IEnumerable<ScheduleResponseDto>>> GetAllAsync()
        {
            var entities = await _db.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v.Company)
                .Include(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations).ThenInclude(dl => dl.Location)
                .Include(s => s.ScheduleSeatClassPricings).ThenInclude(p => p.SeatClass)
                .Include(s => s.ScheduleStatus)
                .ToListAsync();
            var dtos = entities.Select(e => MapToDto(e)).ToList();
            return ApiResponse<IEnumerable<ScheduleResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<ScheduleResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _db.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v.Company)
                .Include(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations).ThenInclude(dl => dl.Location)
                .Include(s => s.ScheduleSeatClassPricings).ThenInclude(p => p.SeatClass)
                .Include(s => s.ScheduleStatus)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);
            if (entity == null) return ApiResponse<ScheduleResponseDto>.Fail("Not found");
            return ApiResponse<ScheduleResponseDto>.Ok(MapToDto(entity));
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        public async Task<ApiResponse<ScheduleLifecycleDto>> GetLifecycleAsync(int? companyId)
        {
            var query = _db.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v.Company)
                .Include(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations).ThenInclude(dl => dl.Location)
                .Include(s => s.ScheduleSeatClassPricings).ThenInclude(p => p.SeatClass)
                .Include(s => s.ScheduleStatus)
                .AsQueryable();

            if (companyId.HasValue && companyId > 0)
            {
                query = query.Where(s => s.Vehicle.CompanyId == companyId.Value);
            }

            var all = await query.OrderByDescending(s => s.DepartureDateTime).ToListAsync();
            var now = DateTime.Now;

            var result = new ScheduleLifecycleDto
            {
                Upcoming    = all.Where(s => s.ScheduleStatus != null &&
                                             (s.ScheduleStatus.ScheduleStatusName == "Scheduled") &&
                                             s.DepartureDateTime > now)
                                 .Select(MapToDto).ToList(),

                Ongoing     = all.Where(s => s.ScheduleStatus != null &&
                                             (s.ScheduleStatus.ScheduleStatusName == "Active" || s.ScheduleStatus.ScheduleStatusName == "Delayed") &&
                                             s.ArrivalDateTime > now)
                                 .Select(MapToDto).ToList(),

                PendingExpiry = all.Where(s => s.ScheduleStatus != null &&
                                               (s.ScheduleStatus.ScheduleStatusName == "Pending Expiry" ||
                                                ((s.ScheduleStatus.ScheduleStatusName == "Active" || s.ScheduleStatus.ScheduleStatusName == "Delayed") && s.ArrivalDateTime <= now)))
                                   .Select(MapToDto).ToList(),

                Expired     = all.Where(s => s.ScheduleStatus != null &&
                                             (s.ScheduleStatus.ScheduleStatusName == "Expired" || 
                                              s.ScheduleStatus.ScheduleStatusName == "Completed" || 
                                              s.ScheduleStatus.ScheduleStatusName == "Cancelled" ||
                                              (s.ScheduleStatus.ScheduleStatusName == "Scheduled" && s.DepartureDateTime <= now)))
                                 .Select(MapToDto).ToList()
            };

            return ApiResponse<ScheduleLifecycleDto>.Ok(result);
        }

        public async Task<ApiResponse<ScheduleResponseDto>> ChangeStatusAsync(int id, string statusName)
        {
            var schedule = await _db.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v.Company)
                .Include(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations).ThenInclude(dl => dl.Location)
                .Include(s => s.ScheduleSeatClassPricings).ThenInclude(p => p.SeatClass)
                .Include(s => s.ScheduleStatus)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);
            if (schedule == null) return ApiResponse<ScheduleResponseDto>.Fail("Schedule not found");

            var status = await _db.ScheduleStatuses
                .FirstOrDefaultAsync(ss => ss.ScheduleStatusName == statusName);
            if (status == null) return ApiResponse<ScheduleResponseDto>.Fail($"Status '{statusName}' not found");

            schedule.ScheduleStatusId = status.ScheduleStatusId;
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();

            // Reload to get fresh nav props
            schedule.ScheduleStatus = status;
            try { await _notificationService.NotifyScheduleChangedAsync(schedule, $"Status changed to {statusName}"); } catch { }
            return ApiResponse<ScheduleResponseDto>.Ok(MapToDto(schedule), $"Status updated to '{statusName}'");
        }

        public async Task<ApiResponse<ScheduleResponseDto>> CloneAsync(ScheduleCloneDto dto)
        {
            // Load the original schedule with all relations
            var original = await _db.Schedules
                .Include(s => s.DepartureLocations)
                .Include(s => s.ScheduleSeatClassPricings)
                .Include(s => s.ScheduleStatus)
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.ScheduleID == dto.OriginalScheduleId);

            if (original == null)
                return ApiResponse<ScheduleResponseDto>.Fail("Original schedule not found");

            // Find the 'Scheduled' status for the new clone
            var scheduledStatus = await _db.ScheduleStatuses
                .FirstOrDefaultAsync(ss => ss.ScheduleStatusName == "Scheduled");
            if (scheduledStatus == null)
                return ApiResponse<ScheduleResponseDto>.Fail("'Scheduled' status not configured in database");

            // Calculate auto base price
            decimal newBasePrice = original.BasePrice;
            if (dto.SeatClassPricings != null && dto.SeatClassPricings.Any())
                newBasePrice = dto.SeatClassPricings.Min(p => p.Price);

            var newSchedule = new Schedule
            {
                RouteId = original.RouteId,
                VehicleId = original.VehicleId,
                DriverInformtionId = dto.DriverInformtionId,
                SupervisorId = dto.SupervisorId,
                DepartureDateTime = dto.DepartureDateTime,
                ArrivalDateTime = dto.ArrivalDateTime,
                BasePrice = newBasePrice,
                AvailableSeats = original.AvailableSeats,
                ScheduleStatusId = scheduledStatus.ScheduleStatusId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newSchedule);
            await _repository.SaveAsync();

            // Copy or override departure locations
            var locations = dto.DepartureLocations != null && dto.DepartureLocations.Any()
                ? dto.DepartureLocations
                : original.DepartureLocations.GroupBy(dl => new { dl.LocationID, dl.Time })
                                              .Select(g => new DepartureLocationDto { LocationID = g.First().LocationID, Time = g.First().Time })
                                              .ToList();
            foreach (var loc in locations)
            {
                _db.DepartureLocations.Add(new DepartureLocation
                {
                    ScheduleID = newSchedule.ScheduleID,
                    LocationID = loc.LocationID,
                    Time = loc.Time
                });
            }

            // Copy or override seat class pricings
            var pricings = dto.SeatClassPricings != null && dto.SeatClassPricings.Any()
                ? dto.SeatClassPricings
                : original.ScheduleSeatClassPricings.GroupBy(p => p.SeatClassId)
                                                     .Select(g => new ScheduleSeatClassPricingDto { SeatClassId = g.First().SeatClassId, Price = g.First().Price })
                                                     .ToList();
            foreach (var p in pricings)
            {
                _db.ScheduleSeatClassPricings.Add(new ScheduleSeatClassPricing
                {
                    ScheduleId = newSchedule.ScheduleID,
                    SeatClassId = p.SeatClassId,
                    Price = p.Price
                });
            }

            await _db.SaveChangesAsync();

            // Seed seat statuses from vehicle seats
            var vehicleSeats = await _db.Seats.Where(s => s.VehicleId == original.VehicleId).ToListAsync();
            var availableStatus = await _db.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
            if (vehicleSeats.Any() && availableStatus != null)
            {
                _db.ScheduleSeatStatuses.AddRange(vehicleSeats.Select(seat => new ScheduleSeatStatus
                {
                    ScheduleID = newSchedule.ScheduleID,
                    SeatID = seat.SeatID,
                    SeatStatusId = availableStatus.SeatStatusId,
                    BookingID = null
                }));
                await _db.SaveChangesAsync();
            }

            // Reload full entity for response
            var freshClone = await _db.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v.Company)
                .Include(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations).ThenInclude(dl => dl.Location)
                .Include(s => s.ScheduleSeatClassPricings).ThenInclude(p => p.SeatClass)
                .Include(s => s.ScheduleStatus)
                .FirstOrDefaultAsync(s => s.ScheduleID == newSchedule.ScheduleID);

            try { await _notificationService.NotifyScheduleChangedAsync(freshClone!, "Cloned"); } catch { }
            return ApiResponse<ScheduleResponseDto>.Ok(MapToDto(freshClone!), "Schedule cloned successfully");
        }

        public async Task<ApiResponse<ScheduleResponseDto>> CreateAsync(ScheduleCreateDto dto)
        {
            var entity = _mapper.Map<Schedule>(dto);

            // Auto-set AvailableSeats from the vehicle's TotalSeats
            var vehicle = await _db.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle != null)
            {
                entity.AvailableSeats = vehicle.TotalSeats;
            }

            await _repository.AddAsync(entity);
            await _repository.SaveAsync();

            // Save departure locations
            if (dto.DepartureLocations != null && dto.DepartureLocations.Count > 0)
            {
                foreach (var loc in dto.DepartureLocations)
                {
                    _db.DepartureLocations.Add(new DepartureLocation 
                    { 
                        ScheduleID = entity.ScheduleID, 
                        LocationID = loc.LocationID,
                        Time = loc.Time 
                    });
                }
                await _db.SaveChangesAsync();
            }

            // Save schedule seat class pricings
            if (dto.SeatClassPricings != null && dto.SeatClassPricings.Any())
            {
                foreach (var p in dto.SeatClassPricings)
                {
                    _db.ScheduleSeatClassPricings.Add(new ScheduleSeatClassPricing
                    {
                        ScheduleId = entity.ScheduleID,
                        SeatClassId = p.SeatClassId,
                        Price = p.Price
                    });
                }
                await _db.SaveChangesAsync();
            }
            else
            {
                // No overrides passed, copy vehicle default seat class pricings if any
                var vehiclePricings = await _db.SeatPricings
                    .Where(vp => vp.VehicleId == dto.VehicleId)
                    .ToListAsync();
                if (vehiclePricings.Any())
                {
                    foreach (var vp in vehiclePricings)
                    {
                        _db.ScheduleSeatClassPricings.Add(new ScheduleSeatClassPricing
                        {
                            ScheduleId = entity.ScheduleID,
                            SeatClassId = vp.SeatClassId,
                            Price = vp.Price
                        });
                    }
                    await _db.SaveChangesAsync();
                }
            }

            // Reload with relations
            entity = await _db.Schedules
                .Include(s => s.Vehicle)
                .Include(s => s.Route)
                    .ThenInclude(r => r.FromLocation)
                .Include(s => s.Route)
                    .ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations)
                    .ThenInclude(dl => dl.Location)
                .Include(s => s.ScheduleSeatClassPricings)
                    .ThenInclude(p => p.SeatClass)
                .FirstOrDefaultAsync(s => s.ScheduleID == entity.ScheduleID);

            // Populate ScheduleSeatStatuses for all seats of the vehicle
            var vehicleSeats = await _db.Seats.Where(s => s.VehicleId == dto.VehicleId).ToListAsync();
            var availableStatus = await _db.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");

            if (vehicleSeats.Any() && availableStatus != null)
            {
                var seatStatuses = vehicleSeats.Select(seat => new ScheduleSeatStatus
                {
                    ScheduleID = entity.ScheduleID,
                    SeatID = seat.SeatID,
                    SeatStatusId = availableStatus.SeatStatusId,
                    BookingID = null
                });
                _db.ScheduleSeatStatuses.AddRange(seatStatuses);
                await _db.SaveChangesAsync();
            }

            try { await _notificationService.NotifyScheduleChangedAsync(entity, "Created"); } catch { }
            return ApiResponse<ScheduleResponseDto>.Ok(MapToDto(entity), "Created successfully");
        }

        public async Task<ApiResponse<ScheduleResponseDto>> UpdateAsync(int id, ScheduleUpdateDto dto)
        {
            var entity = await _db.Schedules
                .Include(s => s.DepartureLocations)
                .Include(s => s.ScheduleSeatClassPricings)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);
            if (entity == null) return ApiResponse<ScheduleResponseDto>.Fail("Not found");
            
            _mapper.Map(dto, entity);

            // Auto-set AvailableSeats from the vehicle's TotalSeats
            var vehicle = await _db.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle != null)
            {
                entity.AvailableSeats = vehicle.TotalSeats;
            }

            _repository.Update(entity);

            // Replace departure locations
            _db.DepartureLocations.RemoveRange(entity.DepartureLocations);
            entity.DepartureLocations.Clear();
            if (dto.DepartureLocations != null && dto.DepartureLocations.Count > 0)
            {
                foreach (var loc in dto.DepartureLocations)
                {
                    entity.DepartureLocations.Add(new DepartureLocation 
                    { 
                        ScheduleID = id, 
                        LocationID = loc.LocationID,
                        Time = loc.Time 
                    });
                }
            }

            // Replace schedule seat class pricings
            _db.ScheduleSeatClassPricings.RemoveRange(entity.ScheduleSeatClassPricings);
            entity.ScheduleSeatClassPricings.Clear();
            if (dto.SeatClassPricings != null && dto.SeatClassPricings.Any())
            {
                foreach (var p in dto.SeatClassPricings)
                {
                    _db.ScheduleSeatClassPricings.Add(new ScheduleSeatClassPricing
                    {
                        ScheduleId = id,
                        SeatClassId = p.SeatClassId,
                        Price = p.Price
                    });
                }
            }

            await _repository.SaveAsync();
            
            // Reload Location navigation properties & SeatClass navigation properties
            entity = await _db.Schedules
                .Include(s => s.Vehicle)
                .Include(s => s.Route)
                    .ThenInclude(r => r.FromLocation)
                .Include(s => s.Route)
                    .ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations)
                    .ThenInclude(dl => dl.Location)
                .Include(s => s.ScheduleSeatClassPricings)
                    .ThenInclude(p => p.SeatClass)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);
            
            try { await _notificationService.NotifyScheduleChangedAsync(entity, "Updated"); } catch { }
            return ApiResponse<ScheduleResponseDto>.Ok(MapToDto(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Schedule not found");

            // 1. Delete ScheduleSeatStatuses linked to this Schedule
            var seatStatuses = await _db.ScheduleSeatStatuses
                .Where(s => s.ScheduleID == id)
                .ToListAsync();
            if (seatStatuses.Any())
                _db.ScheduleSeatStatuses.RemoveRange(seatStatuses);

            // 2. Load all Bookings for this Schedule with their children
            var bookings = await _db.Bookings
                .Include(b => b.BookingSeats)
                .Include(b => b.Tickets)
                .Include(b => b.Payments)
                    .ThenInclude(p => p.PaymentCancels)
                .Include(b => b.Refunds)
                .Where(b => b.ScheduleID == id)
                .ToListAsync();

            foreach (var booking in bookings)
            {
                if (booking.Refunds != null && booking.Refunds.Any())
                    _db.Refunds.RemoveRange(booking.Refunds);

                if (booking.Payments != null && booking.Payments.Any())
                {
                    foreach(var payment in booking.Payments)
                    {
                        if (payment.PaymentCancels != null && payment.PaymentCancels.Any())
                            _db.PaymentCancels.RemoveRange(payment.PaymentCancels);
                    }
                    _db.Payments.RemoveRange(booking.Payments);
                }

                if (booking.Tickets != null && booking.Tickets.Any())
                    _db.Tickets.RemoveRange(booking.Tickets);
                
                if (booking.BookingSeats != null && booking.BookingSeats.Any())
                    _db.BookingSeats.RemoveRange(booking.BookingSeats);
            }
            if (bookings.Any())
                _db.Bookings.RemoveRange(bookings);

            // 3. Delete DepartureLocations linked to this Schedule
            var departureLocations = await _db.DepartureLocations
                .Where(dl => dl.ScheduleID == id)
                .ToListAsync();
            if (departureLocations.Any())
                _db.DepartureLocations.RemoveRange(departureLocations);

            // 4. Delete ScheduleSeatClassPricings linked to this Schedule
            var schedulePricings = await _db.ScheduleSeatClassPricings
                .Where(p => p.ScheduleId == id)
                .ToListAsync();
            if (schedulePricings.Any())
                _db.ScheduleSeatClassPricings.RemoveRange(schedulePricings);

            // 5. Now safe to delete the Schedule
            _repository.Remove(entity);
            await _repository.SaveAsync();

            try { await _notificationService.NotifyScheduleChangedAsync(entity, "Deleted"); } catch { }
            return ApiResponse<bool>.Ok(true, "Schedule and all related records deleted successfully");
        }

        private ScheduleResponseDto MapToDto(Schedule s)
        {
            var dto = _mapper.Map<ScheduleResponseDto>(s);

            // Identity & status enrichment
            dto.VehicleName         = s.Vehicle?.VehicleName;
            dto.VehicleNumber       = s.Vehicle?.VehicleNumber;
            dto.ScheduleStatusName  = s.ScheduleStatus?.ScheduleStatusName;
            dto.CompanyId           = s.Vehicle?.CompanyId;
            dto.CompanyName         = s.Vehicle?.Company?.CompanyName;
            dto.CompanyLogo         = s.Vehicle?.Company?.LogoURL;

            dto.DepartureLocations = s.DepartureLocations?
                .GroupBy(dl => new { dl.LocationID, dl.Time })
                .Select(g => g.First())
                .Select(dl => new DepartureLocationDto
                {
                    LocationID = dl.LocationID,
                    LocationName = dl.Location?.LocationName,
                    Time = dl.Time,
                    Latitude = dl.Location?.Latitude,
                    Longitude = dl.Location?.Longitude
                }).ToList() ?? new List<DepartureLocationDto>();
            
            dto.SeatClassPricings = s.ScheduleSeatClassPricings?
                .GroupBy(p => p.SeatClassId)
                .Select(g => g.First())
                .Select(p => new ScheduleSeatClassPricingDto
                {
                    SeatClassId = p.SeatClassId,
                    SeatClassName = p.SeatClass?.SeatClassName,
                    Price = p.Price
                }).ToList() ?? new List<ScheduleSeatClassPricingDto>();

            return dto;
        }
    }
}