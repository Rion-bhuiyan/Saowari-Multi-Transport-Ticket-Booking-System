using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Route;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saowari.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRepository<Saowari.Models.Entities.Route> _repository;
        private readonly SaowariDbContext _context;
        private readonly IMapper _mapper;

        public RouteService(IRepository<Saowari.Models.Entities.Route> repository, SaowariDbContext context, IMapper mapper)
        {
            _repository = repository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<RouteResponseDto>>> GetAllAsync()
        {
            var entities = await _context.Routes
                .ToListAsync();

            var dtos = entities.Select(r => MapToDto(r)).ToList();
            return ApiResponse<IEnumerable<RouteResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<RouteResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _context.Routes
                .FirstOrDefaultAsync(r => r.RouteID == id);

            if (entity == null) return ApiResponse<RouteResponseDto>.Fail("Not found");
            return ApiResponse<RouteResponseDto>.Ok(MapToDto(entity));
        }

        public async Task<ApiResponse<RouteResponseDto>> CreateAsync(RouteCreateDto dto)
        {
            var entity = _mapper.Map<Saowari.Models.Entities.Route>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveAsync();

            return ApiResponse<RouteResponseDto>.Ok(MapToDto(entity), "Created successfully");
        }

        public async Task<ApiResponse<RouteResponseDto>> UpdateAsync(int id, RouteUpdateDto dto)
        {
            var entity = await _context.Routes
                .FirstOrDefaultAsync(r => r.RouteID == id);

            if (entity == null) return ApiResponse<RouteResponseDto>.Fail("Not found");

            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();

            return ApiResponse<RouteResponseDto>.Ok(MapToDto(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");

            try
            {
                // Load all schedules for this route with their dependent data
                var schedules = await _context.Schedules
                    .Include(s => s.Bookings)
                        .ThenInclude(b => b.BookingSeats)
                    .Include(s => s.Bookings)
                        .ThenInclude(b => b.Tickets)
                    .Include(s => s.Bookings)
                        .ThenInclude(b => b.Payments)
                            .ThenInclude(p => p.PaymentCancels)
                    .Include(s => s.Bookings)
                        .ThenInclude(b => b.Refunds)
                    .Where(s => s.RouteId == id)
                    .ToListAsync();

                foreach (var schedule in schedules)
                {
                    // Delete seat statuses
                    var seatStatuses = await _context.ScheduleSeatStatuses
                        .Where(ss => ss.ScheduleID == schedule.ScheduleID).ToListAsync();
                    if (seatStatuses.Any()) _context.ScheduleSeatStatuses.RemoveRange(seatStatuses);

                    // Delete departure locations for this schedule
                    var depLocs = await _context.DepartureLocations
                        .Where(dl => dl.ScheduleID == schedule.ScheduleID).ToListAsync();
                    if (depLocs.Any()) _context.DepartureLocations.RemoveRange(depLocs);

                    // Delete booking children
                    foreach (var booking in schedule.Bookings)
                    {
                        if (booking.Refunds?.Any() == true)
                            _context.Refunds.RemoveRange(booking.Refunds);

                        if (booking.Payments?.Any() == true)
                        {
                            foreach (var payment in booking.Payments)
                            {
                                if (payment.PaymentCancels?.Any() == true)
                                    _context.PaymentCancels.RemoveRange(payment.PaymentCancels);
                            }
                            _context.Payments.RemoveRange(booking.Payments);
                        }

                        if (booking.Tickets?.Any() == true)
                            _context.Tickets.RemoveRange(booking.Tickets);

                        if (booking.BookingSeats?.Any() == true)
                            _context.BookingSeats.RemoveRange(booking.BookingSeats);
                    }

                    if (schedule.Bookings?.Any() == true)
                        _context.Bookings.RemoveRange(schedule.Bookings);
                }

                if (schedules.Any())
                    _context.Schedules.RemoveRange(schedules);

                // Now delete the route
                _repository.Remove(entity);
                await _repository.SaveAsync();

                return ApiResponse<bool>.Ok(true, "Route and all related data deleted successfully");
            }
            catch (System.Exception ex)
            {
                return ApiResponse<bool>.Fail($"An error occurred while deleting: {ex.Message}");
            }
        }

        private RouteResponseDto MapToDto(Saowari.Models.Entities.Route r)
        {
            var dto = _mapper.Map<RouteResponseDto>(r);
            return dto;
        }
    }
}