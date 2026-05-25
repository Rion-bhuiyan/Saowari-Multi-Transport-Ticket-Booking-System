using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Entities;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatsAvailabilityController : ControllerBase
    {
        private readonly SaowariDbContext _context;

        public SeatsAvailabilityController(SaowariDbContext context)
        {
            _context = context;
        }

        // GET: api/SeatsAvailability/{scheduleId}
        [HttpGet("{scheduleId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetSeatAvailability(int scheduleId)
        {
            // Fetch the schedule to ensure it exists and get vehicle info
            var schedule = await _context.Schedules
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v.Seats)
                .FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);

            if (schedule == null)
            {
                return NotFound(new { Message = "Schedule not found." });
            }

            // Fetch the statuses of seats for this schedule
            var seatStatuses = await _context.ScheduleSeatStatuses
                .Include(ss => ss.SeatStatus)
                .Where(ss => ss.ScheduleID == scheduleId)
                .ToDictionaryAsync(ss => ss.SeatID, ss => ss.SeatStatus?.StatusName ?? "Available");

            // Build the response with real-time seat availability
            var availability = schedule.Vehicle.Seats.Select(seat => new
            {
                SeatId = seat.SeatID,
                SeatNumber = seat.SeatNumber,
                SeatClass = seat.SeatClassId, // Could include SeatClass name here
                Status = seatStatuses.ContainsKey(seat.SeatID) ? seatStatuses[seat.SeatID] : "Available"
            });

            return Ok(availability);
        }
    }
}
