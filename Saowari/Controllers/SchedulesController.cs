using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Schedule;
using Saowari.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Models.Entities;
using Saowari.Models.DTOs.Business;

namespace Saowari.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _service;
        private readonly SaowariDbContext _context;

        public SchedulesController(IScheduleService service, SaowariDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "ScheduleViewer")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (!result.Success) return BadRequest(result);

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var validVehicleIds = await _context.Vehicles
                        .Where(v => v.CompanyId == companyId)
                        .Select(v => v.VehicleID)
                        .ToListAsync();
                    
                    result.Data = result.Data.Where(s => validVehicleIds.Contains(s.VehicleId)).ToList();
                }
            }
            else if (userRole == "Supervisor")
            {
                var supervisorIdClaim = User.FindFirst("SupervisorId")?.Value;
                if (int.TryParse(supervisorIdClaim, out int supervisorId))
                {
                    result.Data = result.Data.Where(s => s.SupervisorId == supervisorId).ToList();
                }
            }
            else if (userRole == "Driver")
            {
                var driverIdClaim = User.FindFirst("DriverInformtionId")?.Value;
                if (int.TryParse(driverIdClaim, out int driverId))
                {
                    result.Data = result.Data.Where(s => s.DriverInformtionId == driverId).ToList();
                }
            }

            return Ok(result);
        }

        [HttpGet("upcoming")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripSearchResultDto>>>> GetUpcomingSchedules()
        {
            var upcomingSchedules = await _context.Schedules
                .Include(s => s.Vehicle).ThenInclude(v => v.VehicleType)
                .Include(s => s.Vehicle).ThenInclude(v => v.Company)
                .Include(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r.ToLocation)
                .Include(s => s.DepartureLocations)
                .Include(s => s.ScheduleStatus)
                .AsNoTracking()
                .Where(s => s.DepartureDateTime >= System.DateTime.Now && (s.ScheduleStatus.ScheduleStatusName == "Active" || s.ScheduleStatus.ScheduleStatusName == "Scheduled"))
                .OrderBy(s => s.DepartureDateTime)
                .Take(6)
                .ToListAsync();

            var mappedResults = upcomingSchedules.Select(s => new TripSearchResultDto
            {
                ScheduleId = s.ScheduleID,
                VehicleId = s.VehicleId,
                VehicleName = s.Vehicle.VehicleName,
                VehicleNumber = s.Vehicle.VehicleNumber,
                VehicleType = s.Vehicle.VehicleType != null ? s.Vehicle.VehicleType.VehicleTypeName : "Unknown",
                CompanyName = s.Vehicle.Company != null ? s.Vehicle.Company.CompanyName : null,
                CompanyLogo = s.Vehicle.Company != null ? s.Vehicle.Company.LogoURL : null,
                SeatLayoutConfig = s.Vehicle.SeatLayoutConfig,
                FromLocation = s.Route.FromLocation != null ? s.Route.FromLocation.LocationName : "Unknown",
                ToLocation = s.Route.ToLocation != null ? s.Route.ToLocation.LocationName : "Unknown",
                DepartureDateTime = s.DepartureDateTime,
                ArrivalDateTime = s.ArrivalDateTime,
                BasePrice = s.BasePrice,
                AvailableSeats = s.AvailableSeats,
                BoardingTime = s.DepartureDateTime
            }).ToList();

            return Ok(ApiResponse<IEnumerable<TripSearchResultDto>>.Ok(mappedResults));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ScheduleResponseDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<ScheduleResponseDto>>> Create([FromBody] ScheduleCreateDto dto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
                    if (vehicle == null || vehicle.CompanyId != companyId)
                    {
                        return Forbid("You can only create schedules for vehicles belonging to your company.");
                    }
                }
            }

            var result = await _service.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.ScheduleID }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<ScheduleResponseDto>>> Update(int id, [FromBody] ScheduleUpdateDto dto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var schedule = await _context.Schedules.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.ScheduleID == id);
                    if (schedule != null && schedule.Vehicle.CompanyId != companyId)
                    {
                        return Forbid("You can only update schedules belonging to your company.");
                    }
                }
            }

            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var schedule = await _context.Schedules.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.ScheduleID == id);
                    if (schedule != null && schedule.Vehicle.CompanyId != companyId)
                    {
                        return Forbid("You can only delete schedules belonging to your company.");
                    }
                }
            }

            var result = await _service.DeleteAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}/seat-map")]
        [Authorize(Policy = "ScheduleViewer")]
        public async Task<IActionResult> GetScheduleSeatMap(int id)
        {
            // Security check
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var schedule = await _context.Schedules
                .Include(s => s.Vehicle)
                .Include(s => s.DriverInformtion)
                    .ThenInclude(di => di.Users)
                .Include(s => s.Supervisor)
                    .ThenInclude(sp => sp.Users)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ScheduleID == id);
            
            if (schedule == null) return NotFound("Schedule not found.");

            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId) && schedule.Vehicle.CompanyId != companyId)
                {
                    return Forbid("Not authorized to view this company's schedule.");
                }
            }
            else if (userRole == "Supervisor")
            {
                var supervisorIdClaim = User.FindFirst("SupervisorId")?.Value;
                if (int.TryParse(supervisorIdClaim, out int supervisorId) && schedule.SupervisorId != supervisorId)
                {
                    return Forbid("Not authorized to view this schedule.");
                }
            }
            else if (userRole == "Driver")
            {
                var driverIdClaim = User.FindFirst("DriverInformtionId")?.Value;
                if (int.TryParse(driverIdClaim, out int driverId) && schedule.DriverInformtionId != driverId)
                {
                    return Forbid("Not authorized to view this schedule.");
                }
            }

            var seatStatuses = await _context.ScheduleSeatStatuses
                .Include(ss => ss.Seat)
                .Include(ss => ss.SeatStatus)
                .AsNoTracking()
                .Where(ss => ss.ScheduleID == id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(b => b.BookingSeats)
                .AsNoTracking()
                .Where(b => b.ScheduleID == id)
                .ToListAsync();

            var seatBookingMap = new Dictionary<int, Booking>();
            foreach (var b in bookings)
            {
                foreach (var bs in b.BookingSeats)
                {
                    if (!seatBookingMap.ContainsKey(bs.SeatId))
                    {
                        seatBookingMap[bs.SeatId] = b;
                    }
                }
            }

            var mappedSeats = seatStatuses.Select(ss => {
                var hasBooking = seatBookingMap.TryGetValue(ss.SeatID, out var booking);
                var isBooked = hasBooking || ss.BookingID != null || ss.SeatStatus?.StatusName == "Booked";
                
                if (!hasBooking && ss.BookingID != null)
                {
                    booking = _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Payments)
                            .ThenInclude(p => p.PaymentMethod)
                        .FirstOrDefault(b => b.BookingID == ss.BookingID);
                }

                object passengerInfo = null;
                if (booking != null)
                {
                    var payment = booking.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                    passengerInfo = new
                    {
                        Name = booking.PassengerName ?? booking.User?.FullName ?? "Unknown Passenger",
                        Phone = booking.PassengerPhone ?? booking.User?.Phone ?? "N/A",
                        Picture = booking.User?.Picture,
                        Email = booking.User?.Email ?? "N/A",
                        AmountPaid = booking.Payments.Any() ? booking.Payments.Sum(p => p.Amount) : booking.FinalAmount,
                        BaseAmount = booking.BaseAmount,
                        DiscountAmount = booking.DiscountAmount,
                        FinalAmount = booking.FinalAmount,
                        PaymentMethod = payment?.PaymentMethod?.PaymentMethodName ?? "Unknown Method",
                        TransactionId = payment?.TransactionID ?? "N/A",
                        PaidAt = payment?.PaidAt,
                        CouponUsed = booking.DiscountAmount > 0
                    };
                }

                return new
                {
                    SeatId = ss.SeatID,
                    SeatNumber = ss.Seat.SeatNumber,
                    Status = isBooked ? "Booked" : (ss.SeatStatus?.StatusName ?? "Available"),
                    IsBooked = isBooked,
                    Passenger = passengerInfo
                };
            }).ToList();

            var driverUser = schedule.DriverInformtion?.Users?.FirstOrDefault();
            var supervisorUser = schedule.Supervisor?.Users?.FirstOrDefault();

            return Ok(new
            {
                ScheduleId = id,
                VehicleName = schedule.Vehicle.VehicleName,
                VehicleNumber = schedule.Vehicle.VehicleNumber,
                SeatLayoutConfig = schedule.Vehicle.SeatLayoutConfig,
                Driver = schedule.DriverInformtion != null ? new
                {
                    Name = driverUser?.FullName ?? "Unknown Driver",
                    Phone = driverUser?.Phone ?? "N/A",
                    Picture = driverUser?.Picture,
                    Email = driverUser?.Email ?? "N/A",
                    LicenceNumber = schedule.DriverInformtion.LicenceNumber,
                    LicenceExpDate = schedule.DriverInformtion.licenceExpDate
                } : null,
                Supervisor = schedule.Supervisor != null ? new
                {
                    Name = supervisorUser?.FullName ?? "Unknown Supervisor",
                    Phone = supervisorUser?.Phone ?? "N/A",
                    Picture = supervisorUser?.Picture,
                    Email = supervisorUser?.Email ?? "N/A"
                } : null,
                Seats = mappedSeats
            });
        }

        // ── Schedule Lifecycle Endpoints ─────────────────────────────────────────

        /// <summary>Returns schedules grouped by lifecycle stage (Upcoming/Ongoing/PendingExpiry/Expired).</summary>
        [HttpGet("lifecycle")]
        [Authorize(Policy = "ScheduleViewer")]
        public async Task<ActionResult<ApiResponse<ScheduleLifecycleDto>>> GetLifecycle([FromQuery] int? companyId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // CompanyManager can only see their own company
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int managedCompanyId))
                    companyId = managedCompanyId;
            }
            // Supervisor sees only schedules assigned to them
            else if (userRole == "Supervisor")
            {
                var supervisorIdClaim = User.FindFirst("SupervisorId")?.Value;
                if (!int.TryParse(supervisorIdClaim, out int _))
                    return Forbid("Supervisor ID not resolved.");
                // companyId filter not applied for supervisors; filtering done in service via SupervisorId
                // For now pass companyId as null and we filter below after result
                var result = await _service.GetLifecycleAsync(null);
                if (!result.Success) return BadRequest(result);
                int supId = int.Parse(supervisorIdClaim!);
                result.Data.Upcoming      = result.Data.Upcoming.Where(s => s.SupervisorId == supId).ToList();
                result.Data.Ongoing       = result.Data.Ongoing.Where(s => s.SupervisorId == supId).ToList();
                result.Data.PendingExpiry = result.Data.PendingExpiry.Where(s => s.SupervisorId == supId).ToList();
                result.Data.Expired       = result.Data.Expired.Where(s => s.SupervisorId == supId).ToList();
                return Ok(result);
            }
            else if (userRole == "Driver")
            {
                var driverIdClaim = User.FindFirst("DriverInformtionId")?.Value;
                if (!int.TryParse(driverIdClaim, out int _))
                    return Forbid("Driver ID not resolved.");
                var result = await _service.GetLifecycleAsync(null);
                if (!result.Success) return BadRequest(result);
                int drvId = int.Parse(driverIdClaim!);
                result.Data.Upcoming      = result.Data.Upcoming.Where(s => s.DriverInformtionId == drvId).ToList();
                result.Data.Ongoing       = result.Data.Ongoing.Where(s => s.DriverInformtionId == drvId).ToList();
                result.Data.PendingExpiry = result.Data.PendingExpiry.Where(s => s.DriverInformtionId == drvId).ToList();
                result.Data.Expired       = result.Data.Expired.Where(s => s.DriverInformtionId == drvId).ToList();
                return Ok(result);
            }

            var lifecycleResult = await _service.GetLifecycleAsync(companyId);
            if (!lifecycleResult.Success) return BadRequest(lifecycleResult);
            return Ok(lifecycleResult);
        }

        /// <summary>Transitions an active/arrived schedule to 'Pending Expiry' for review.</summary>
        [HttpPatch("{id}/mark-pending")]
        [Authorize(Policy = "ScheduleViewer")]
        public async Task<ActionResult<ApiResponse<ScheduleResponseDto>>> MarkPendingExpiry(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var schedule = await _context.Schedules.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.ScheduleID == id);
                    if (schedule != null && schedule.Vehicle.CompanyId != companyId)
                        return Forbid("You can only manage schedules belonging to your company.");
                }
            }
            else if (userRole == "Supervisor")
            {
                var supervisorIdClaim = User.FindFirst("SupervisorId")?.Value;
                if (int.TryParse(supervisorIdClaim, out int supervisorId))
                {
                    var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleID == id);
                    if (schedule != null && schedule.SupervisorId != supervisorId)
                        return Forbid("You can only manage your assigned schedules.");
                }
            }
            else if (userRole == "Driver")
            {
                var driverIdClaim = User.FindFirst("DriverInformtionId")?.Value;
                if (int.TryParse(driverIdClaim, out int driverId))
                {
                    var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleID == id);
                    if (schedule != null && schedule.DriverInformtionId != driverId)
                        return Forbid("You can only manage your assigned schedules.");
                }
            }

            var result = await _service.ChangeStatusAsync(id, "Pending Expiry");
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>Approves the expiry of a 'Pending Expiry' schedule — moves it to the Expired archive.</summary>
        [HttpPatch("{id}/approve-expiry")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<ScheduleResponseDto>>> ApproveExpiry(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var schedule = await _context.Schedules.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.ScheduleID == id);
                    if (schedule != null && schedule.Vehicle.CompanyId != companyId)
                        return Forbid("You can only approve expiry for your own company's schedules.");
                }
            }

            var result = await _service.ChangeStatusAsync(id, "Expired");
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>Clones an Expired schedule into a new Scheduled one, requiring new driver, supervisor, and dates.</summary>
        [HttpPost("clone")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<ApiResponse<ScheduleResponseDto>>> CloneSchedule([FromBody] ScheduleCloneDto dto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if ((userRole == "CompanyManager" || userRole == "Manager"))
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (int.TryParse(companyIdClaim, out int companyId))
                {
                    var original = await _context.Schedules.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.ScheduleID == dto.OriginalScheduleId);
                    if (original != null && original.Vehicle.CompanyId != companyId)
                        return Forbid("You can only clone schedules belonging to your company.");
                }
            }

            var result = await _service.CloneAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.ScheduleID }, result);
        }
    }
}
