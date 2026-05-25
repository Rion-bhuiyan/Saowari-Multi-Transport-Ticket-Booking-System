using Microsoft.EntityFrameworkCore;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Business;
using Saowari.Models.DTOs.Ticket;
using Saowari.Models.Entities;
using Saowari.Models.Responses;

namespace Saowari.Services.BusinessServices
{
    public class SearchService : ISearchService
    {
        private readonly SaowariDbContext _context;
        public SearchService(SaowariDbContext context) { _context = context; }

        public async Task<ApiResponse<IEnumerable<TripSearchResult>>> SearchTripsAsync(string transportType, int fromLocationId, int toLocationId, DateTime travelDate, int passengers, int? seatClassId)
        {
            var results = await _context.Schedules
                .Where(s => (s.ScheduleStatus.ScheduleStatusName == "Active" || s.ScheduleStatus.ScheduleStatusName == "Scheduled")
                            && (s.Route.FromLocationID == fromLocationId || s.DepartureLocations.Any(dl => dl.LocationID == fromLocationId))
                            && s.Route.ToLocationID == toLocationId
                            && s.DepartureDateTime.Date == travelDate.Date
                            && s.AvailableSeats >= passengers
                            && (string.IsNullOrEmpty(transportType) || 
                                (s.Vehicle != null && s.Vehicle.Company != null && s.Vehicle.Company.CompanyType != null && s.Vehicle.Company.CompanyType.CompanyTypeName.Contains(transportType)) ||
                                (s.Vehicle != null && s.Vehicle.VehicleType != null && s.Vehicle.VehicleType.VehicleTypeName.Contains(transportType))))
                .Include(s => s.DepartureLocations)
                .Include(s => s.Vehicle).ThenInclude(v => v.VehicleType)
                .Include(s => s.Vehicle).ThenInclude(v => v.Company)
                .Include(s => s.Route).ThenInclude(r => r.FromLocation)
                .Include(s => s.Route).ThenInclude(r => r.ToLocation)
                .ToListAsync();

            var mappedResults = results.Select(s => {
                var boardingTime = s.DepartureDateTime;
                var dl = s.DepartureLocations.FirstOrDefault(d => d.LocationID == fromLocationId);
                if (dl != null)
                {
                    boardingTime = s.DepartureDateTime.Date + dl.Time;
                }

                return new TripSearchResult
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
                    BoardingTime = boardingTime
                };
            }).ToList();

            return ApiResponse<IEnumerable<TripSearchResult>>.Ok(mappedResults);
        }

        public async Task<ApiResponse<object>> GetSeatMapAsync(int scheduleId)
        {
            // Lazy-initialize: if no seat statuses exist for this schedule, create them now
            bool hasStatuses = await _context.ScheduleSeatStatuses.AnyAsync(s => s.ScheduleID == scheduleId);
            if (!hasStatuses)
            {
                var schedule = await _context.Schedules
                    .Include(s => s.Vehicle).ThenInclude(v => v.Seats)
                    .FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);

                var availableStatus = await _context.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");

                if (schedule != null && availableStatus != null && schedule.Vehicle?.Seats?.Any() == true)
                {
                    var newStatuses = schedule.Vehicle.Seats.Select(seat => new ScheduleSeatStatus
                    {
                        ScheduleID = scheduleId,
                        SeatID = seat.SeatID,
                        SeatStatusId = availableStatus.SeatStatusId,
                        BookingID = null
                    });
                    _context.ScheduleSeatStatuses.AddRange(newStatuses);
                    await _context.SaveChangesAsync();
                }
            }

            var pricings = await _context.ScheduleSeatClassPricings
                .Where(p => p.ScheduleId == scheduleId)
                .ToDictionaryAsync(p => p.SeatClassId, p => p.Price);

            var scheduleObj = await _context.Schedules.FindAsync(scheduleId);
            decimal basePrice = scheduleObj?.BasePrice ?? 0;

            var rawSeats = await _context.ScheduleSeatStatuses
                .Include(s => s.Seat).ThenInclude(seat => seat.SeatClass)
                .Include(s => s.SeatStatus)
                .Where(s => s.ScheduleID == scheduleId)
                .Select(s => new 
                {
                    StatusId = s.StatusID,
                    ScheduleId = s.ScheduleID,
                    SeatId = s.SeatID,
                    SeatNumber = s.Seat.SeatNumber,
                    SeatStatusName = s.SeatStatus.StatusName,
                    IsBooked = s.BookingID != null,
                    BookingId = s.BookingID,
                    SeatClassId = s.Seat.SeatClassId,
                    SeatClassName = s.Seat.SeatClass != null ? s.Seat.SeatClass.SeatClassName : "Standard"
                })
                .ToListAsync();

            var seats = rawSeats.Select(s => new 
            {
                s.StatusId,
                s.ScheduleId,
                s.SeatId,
                s.SeatNumber,
                s.SeatStatusName,
                s.IsBooked,
                s.BookingId,
                s.SeatClassId,
                s.SeatClassName,
                Price = pricings.TryGetValue(s.SeatClassId, out var customPrice) ? customPrice : basePrice
            }).ToList();

            return ApiResponse<object>.Ok(seats);
        }
    }

    public class BookingFlowService : IBookingFlowService
    {
        private readonly SaowariDbContext _context;
        public BookingFlowService(SaowariDbContext context) { _context = context; }

        public async Task<ApiResponse<object>> ValidateSeatsAsync(int scheduleId, List<int> seatIds)
        {
            var seatStatuses = await _context.ScheduleSeatStatuses
                .Include(s => s.SeatStatus)
                .Where(s => s.ScheduleID == scheduleId && seatIds.Contains(s.SeatID))
                .ToListAsync();

            var unavailable = seatStatuses.Where(s => s.SeatStatus.StatusName != "Available").Select(s => s.SeatID).ToList();

            return ApiResponse<object>.Ok(new { AllAvailable = !unavailable.Any(), UnavailableSeats = unavailable });
        }

        public async Task<ApiResponse<FareSummaryDto>> GetFareSummaryAsync(int scheduleId, List<int> seatIds, int? discountId)
        {
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null) return ApiResponse<FareSummaryDto>.Fail("Schedule not found");

            var pricings = await _context.ScheduleSeatClassPricings
                .Where(p => p.ScheduleId == scheduleId)
                .ToDictionaryAsync(p => p.SeatClassId, p => p.Price);

            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.SeatID))
                .ToListAsync();

            decimal baseAmount = 0;
            foreach (var seat in seats)
            {
                if (pricings.TryGetValue(seat.SeatClassId, out var customPrice))
                {
                    baseAmount += customPrice;
                }
                else
                {
                    baseAmount += schedule.BasePrice;
                }
            }

            decimal discountAmount = 0;
            string? discountName = null;

            if (discountId.HasValue)
            {
                var discount = await _context.Discounts.FindAsync(discountId.Value);
                if (discount != null && discount.IsActive && discount.StartDate <= DateTime.UtcNow && discount.EndDate >= DateTime.UtcNow)
                {
                    // Logic simplification
                    discountAmount = discount.DiscountValue; // Simplified
                    discountName = discount.DiscountName;
                }
            }

            return ApiResponse<FareSummaryDto>.Ok(new FareSummaryDto
            {
                BaseAmount = baseAmount,
                DiscountAmount = discountAmount,
                FinalAmount = baseAmount - discountAmount,
                DiscountName = discountName
            });
        }

        public async Task<ApiResponse<object>> RescheduleAsync(int bookingId, int newScheduleId, List<int> newSeatIds)
        {
            return ApiResponse<object>.Fail("Not implemented completely");
        }
    }

    public class RefundCalculationService : IRefundCalculationService
    {
        private readonly SaowariDbContext _context;
        private readonly INotificationService _notificationService;
        
        public RefundCalculationService(SaowariDbContext context, INotificationService notificationService) 
        { 
            _context = context; 
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<RefundPreviewDto>> CalculateRefundAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Vehicle)
                .Include(b => b.BookingStatus)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
                return ApiResponse<RefundPreviewDto>.Fail("Booking not found.");

            if (booking.BookingStatus?.BookingStatusName == "Cancelled")
                return ApiResponse<RefundPreviewDto>.Fail("This booking has already been cancelled.");

            var departureTime = booking.Schedule?.DepartureDateTime ?? DateTime.UtcNow.AddHours(25);
            var hoursUntilDeparture = (departureTime - DateTime.UtcNow).TotalHours;

            if (hoursUntilDeparture < 0)
                return ApiResponse<RefundPreviewDto>.Fail("Cannot request a refund for a trip that has already departed.");

            decimal percentage = 0;
            string policyName = "No Refund Policy";
            int policyId = 1;

            var companyId = booking.Schedule?.Vehicle?.CompanyId;
            if (companyId.HasValue)
            {
                var policies = await _context.RefundPolicies
                    .Where(rp => rp.CompanyId == companyId.Value && rp.IsActive)
                    .OrderByDescending(rp => rp.HoursBeforeDeparture)
                    .ToListAsync();

                if (policies.Any())
                {
                    var matchedPolicy = policies.FirstOrDefault(rp => hoursUntilDeparture >= rp.HoursBeforeDeparture);
                    if (matchedPolicy != null)
                    {
                        percentage = matchedPolicy.RefundPercentage;
                        policyName = matchedPolicy.PolicyName;
                        policyId = matchedPolicy.PolicyID;
                    }
                    else
                    {
                        percentage = 0;
                        policyName = "Non-Refundable (Late Cancellation)";
                        policyId = policies.Last().PolicyID;
                    }
                }
                else
                {
                    if (hoursUntilDeparture >= 72)
                    {
                        percentage = 100;
                        policyName = "Full Refund (72+ Hours Before)";
                    }
                    else if (hoursUntilDeparture >= 24)
                    {
                        percentage = 50;
                        policyName = "Partial Refund (24 to 72 Hours Before)";
                    }
                    else
                    {
                        percentage = 0;
                        policyName = "Non-Refundable (Less than 24 Hours)";
                    }
                }
            }
            else
            {
                if (hoursUntilDeparture >= 72)
                {
                    percentage = 100;
                    policyName = "Full Refund (72+ Hours Before)";
                }
                else if (hoursUntilDeparture >= 24)
                {
                    percentage = 50;
                    policyName = "Partial Refund (24 to 72 Hours Before)";
                }
                else
                {
                    percentage = 0;
                    policyName = "Non-Refundable (Less than 24 Hours)";
                }
            }

            // Refundable base excludes non-refundable processing fees and VAT
            var originalAmount = booking.BaseAmount - booking.DiscountAmount;
            var eligibleAmount = originalAmount * (percentage / 100);

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
            int paymentId = payment?.PaymentID ?? 0;

            var preview = new RefundPreviewDto
            {
                BookingId = bookingId,
                PaymentId = paymentId,
                HoursUntilDeparture = Math.Round(hoursUntilDeparture, 1),
                PolicyName = policyName,
                RefundPercentage = percentage,
                EligibleRefundAmount = eligibleAmount,
                OriginalAmount = originalAmount,
                Message = percentage > 0 
                    ? $"You are eligible for a {percentage}% refund of ৳{eligibleAmount} based on the cancellation policy."
                    : "This booking is non-refundable because it is less than 24 hours before departure."
            };

            return ApiResponse<RefundPreviewDto>.Ok(preview);
        }

        public async Task<ApiResponse<Models.DTOs.Refund.RefundResponseDto>> RequestRefundAsync(int bookingId, string remarks, int userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Vehicle)
                .Include(b => b.BookingStatus)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
                return ApiResponse<Models.DTOs.Refund.RefundResponseDto>.Fail("Booking not found.");

            if (booking.BookingStatus?.BookingStatusName == "Cancelled")
                return ApiResponse<Models.DTOs.Refund.RefundResponseDto>.Fail("This booking has already been cancelled.");

            var departureTime = booking.Schedule?.DepartureDateTime ?? DateTime.UtcNow;
            var hoursUntilDeparture = (departureTime - DateTime.UtcNow).TotalHours;

            if (hoursUntilDeparture < 0)
                return ApiResponse<Models.DTOs.Refund.RefundResponseDto>.Fail("Cannot request a refund for a trip that has already departed.");

            decimal percentage = 0;
            string policyName = "No Refund Policy";
            int policyId = 1;

            var companyId = booking.Schedule?.Vehicle?.CompanyId;
            if (companyId.HasValue)
            {
                var policies = await _context.RefundPolicies
                    .Where(rp => rp.CompanyId == companyId.Value && rp.IsActive)
                    .OrderByDescending(rp => rp.HoursBeforeDeparture)
                    .ToListAsync();

                if (policies.Any())
                {
                    var matchedPolicy = policies.FirstOrDefault(rp => hoursUntilDeparture >= rp.HoursBeforeDeparture);
                    if (matchedPolicy != null)
                    {
                        percentage = matchedPolicy.RefundPercentage;
                        policyName = matchedPolicy.PolicyName;
                        policyId = matchedPolicy.PolicyID;
                    }
                    else
                    {
                        percentage = 0;
                        policyName = "Non-Refundable (Late Cancellation)";
                        policyId = policies.Last().PolicyID;
                    }
                }
                else
                {
                    if (hoursUntilDeparture >= 72)
                    {
                        percentage = 100;
                        policyName = "Full Refund (72+ Hours Before)";
                    }
                    else if (hoursUntilDeparture >= 24)
                    {
                        percentage = 50;
                        policyName = "Partial Refund (24 to 72 Hours Before)";
                    }
                    else
                    {
                        percentage = 0;
                        policyName = "Non-Refundable (Less than 24 Hours)";
                    }
                }
            }
            else
            {
                if (hoursUntilDeparture >= 72)
                {
                    percentage = 100;
                    policyName = "Full Refund (72+ Hours Before)";
                }
                else if (hoursUntilDeparture >= 24)
                {
                    percentage = 50;
                    policyName = "Partial Refund (24 to 72 Hours Before)";
                }
                else
                {
                    percentage = 0;
                    policyName = "Non-Refundable (Less than 24 Hours)";
                }
            }

            // Refundable base excludes non-refundable processing fees and VAT
            var originalAmount = booking.BaseAmount - booking.DiscountAmount;
            var eligibleAmount = originalAmount * (percentage / 100);

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
            if (payment == null)
            {
                payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = originalAmount,
                    PaymentMethodId = 1,
                    PaymentStatusId = 2,
                    PaidAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }

            var refundStatus = await _context.RefundStatuses.FirstOrDefaultAsync(rs => rs.StatusName == "Requested")
                ?? await _context.RefundStatuses.FirstOrDefaultAsync();

            int refundStatusId = refundStatus?.RefundStatusId ?? 1;

            var refund = new Refund
            {
                BookingId = bookingId,
                PaymentId = payment.PaymentID,
                RequestedAt = DateTime.UtcNow,
                RefundPercentage = percentage,
                RefundAmount = eligibleAmount,
                RefundStatusId = refundStatusId,
                PolicyID = policyId,
                Remarks = remarks,
                IsRefunded = false
            };

            _context.Refunds.Add(refund);

            var seatStatuses = await _context.ScheduleSeatStatuses
                .Where(sss => sss.BookingID == bookingId)
                .ToListAsync();

            var availableSeatStatus = await _context.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
            if (availableSeatStatus != null)
            {
                foreach (var ss in seatStatuses)
                {
                    ss.BookingID = null;
                    ss.SeatStatusId = availableSeatStatus.SeatStatusId;
                }
            }

            var cancelledBookingStatus = await _context.BookingStatuses.FirstOrDefaultAsync(bs => bs.BookingStatusName == "Cancelled");
            if (cancelledBookingStatus != null)
            {
                booking.BookingStatusId = cancelledBookingStatus.BookingStatusId;
            }
            else
            {
                booking.BookingStatusId = 3;
            }

            await _context.SaveChangesAsync();

            var responseDto = new Models.DTOs.Refund.RefundResponseDto
            {
                RefundID = refund.RefundID,
                BookingId = refund.BookingId,
                PaymentId = refund.PaymentId,
                RequestedAt = refund.RequestedAt,
                RefundPercentage = refund.RefundPercentage,
                RefundAmount = refund.RefundAmount,
                RefundStatusId = refund.RefundStatusId,
                ProcessedAt = refund.ProcessedAt,
                RefundTransactionID = refund.RefundTransactionID,
                Remarks = refund.Remarks,
                IsRefunded = refund.IsRefunded,
                PolicyID = refund.PolicyID
            };

            try { await _notificationService.NotifyRefundRequestedAsync(refund); } catch { }

            return ApiResponse<Models.DTOs.Refund.RefundResponseDto>.Ok(responseDto, "Refund requested successfully.");
        }
    }

    public class TicketBusinessService : ITicketBusinessService
    {
        private readonly SaowariDbContext _context;
        public TicketBusinessService(SaowariDbContext context) { _context = context; }

        public async Task<ApiResponse<IEnumerable<TicketResponseDto>>> IssueTicketsForBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .Include(b => b.BookingStatus)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
                return ApiResponse<IEnumerable<TicketResponseDto>>.Fail("Booking not found");

            if (booking.BookingStatus?.BookingStatusName != "Confirmed")
                return ApiResponse<IEnumerable<TicketResponseDto>>.Fail("Tickets can only be issued for Confirmed bookings");

            // Avoid re-issuing tickets
            var existing = await _context.Tickets.AnyAsync(t => t.BookingId == bookingId);
            if (existing)
                return ApiResponse<IEnumerable<TicketResponseDto>>.Fail("Tickets have already been issued for this booking");

            var tickets = new List<Ticket>();
            foreach (var bs in booking.BookingSeats)
            {
                var seatLabel = bs.Seat?.SeatNumber ?? bs.SeatId.ToString();
                var ticket = new Ticket
                {
                    BookingId  = bookingId,
                    TicketCode = $"TKT-{booking.BookingCode}-{seatLabel}",
                    IssuedAt   = DateTime.UtcNow,
                    IsUsed     = false
                };
                tickets.Add(ticket);
                _context.Tickets.Add(ticket);
            }

            await _context.SaveChangesAsync();

            var dtos = tickets.Select(t => new TicketResponseDto
            {
                TicketID   = t.TicketID,
                BookingId  = t.BookingId,
                TicketCode = t.TicketCode,
                IssuedAt   = t.IssuedAt,
                IsUsed     = t.IsUsed,
                UsedAt     = t.UsedAt
            });

            return ApiResponse<IEnumerable<TicketResponseDto>>.Ok(dtos, $"{tickets.Count} ticket(s) issued successfully");
        }

        public async Task<ApiResponse<TicketVerificationDto>> VerifyTicketAsync(string ticketCode)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Booking)
                    .ThenInclude(b => b!.BookingSeats)
                        .ThenInclude(bs => bs.Seat)
                .Include(t => t.Booking)
                    .ThenInclude(b => b!.Schedule)
                        .ThenInclude(s => s!.Route)
                            .ThenInclude(r => r!.FromLocation)
                .Include(t => t.Booking)
                    .ThenInclude(b => b!.Schedule)
                        .ThenInclude(s => s!.Route)
                            .ThenInclude(r => r!.ToLocation)
                .Include(t => t.Booking)
                    .ThenInclude(b => b!.Schedule)
                        .ThenInclude(s => s!.Vehicle)
                .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);

            if (ticket == null)
                return ApiResponse<TicketVerificationDto>.Ok(new TicketVerificationDto
                {
                    IsValid    = false,
                    TicketCode = ticketCode,
                    Status     = "Invalid — ticket code not found"
                }, "Ticket not found");

            var booking  = ticket.Booking!;
            var schedule = booking.Schedule!;
            var route    = schedule.Route!;

            var seatNumbers = booking.BookingSeats
                .Select(bs => bs.Seat?.SeatNumber ?? bs.SeatId.ToString())
                .ToList();

            var dto = new TicketVerificationDto
            {
                IsValid           = true,
                TicketCode        = ticket.TicketCode,
                PassengerName     = booking.PassengerName,
                SeatNumber        = string.Join(", ", seatNumbers),
                Route             = $"{route.FromLocation?.LocationName} → {route.ToLocation?.LocationName}",
                DepartureDateTime = schedule.DepartureDateTime,
                VehicleName       = schedule.Vehicle?.VehicleName ?? "N/A",
                IsUsed            = ticket.IsUsed,
                Status            = ticket.IsUsed
                    ? $"Already used at {ticket.UsedAt:yyyy-MM-dd HH:mm} UTC"
                    : "Valid — not yet scanned"
            };

            return ApiResponse<TicketVerificationDto>.Ok(dto, "Ticket verified successfully");
        }

        public async Task<ApiResponse<object>> ScanTicketAsync(string ticketCode)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);

            if (ticket == null)
                return ApiResponse<object>.Fail("Ticket not found");

            if (ticket.IsUsed)
                return ApiResponse<object>.Fail($"Ticket already scanned at {ticket.UsedAt:yyyy-MM-dd HH:mm} UTC");

            ticket.IsUsed = true;
            ticket.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<object>.Ok(new
            {
                TicketCode = ticket.TicketCode,
                ScannedAt  = ticket.UsedAt
            }, "Ticket scanned and marked as used");
        }
    }

    public class DashboardService : IDashboardService
    {
        private readonly SaowariDbContext _context;
        public DashboardService(SaowariDbContext context) { _context = context; }

        public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfToday = today;
            var endOfToday = today.AddDays(1).AddTicks(-1);

            var todayRevenue = await _context.Payments
                .Include(p => p.PaymentStatus)
                .Where(p => (p.PaymentStatus.PaymentStatusName == "Completed" || p.PaymentStatus.PaymentStatusName == "Paid") 
                    && p.CreatedAt >= startOfToday && p.CreatedAt <= endOfToday)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var summary = new DashboardSummaryDto
            {
                TodayBookingsCount = await _context.Bookings.CountAsync(b => b.BookingDate >= startOfToday && b.BookingDate <= endOfToday),
                TodayRevenue = todayRevenue,
                TotalActiveRoutes = await _context.Routes.CountAsync(r => r.IsActive),
                TotalActiveSchedules = await _context.Schedules.CountAsync(s => s.ScheduleStatus != null && 
                    (s.ScheduleStatus.ScheduleStatusName == "Active" || s.ScheduleStatus.ScheduleStatusName == "Scheduled")),
                UpcomingDeparturesToday = await _context.Schedules.CountAsync(s => s.DepartureDateTime >= DateTime.UtcNow && s.DepartureDateTime <= endOfToday && s.ScheduleStatus != null &&
                    (s.ScheduleStatus.ScheduleStatusName == "Active" || s.ScheduleStatus.ScheduleStatusName == "Scheduled"))
            };
            return ApiResponse<DashboardSummaryDto>.Ok(summary);
        }

        public async Task<ApiResponse<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate, string groupBy)
        {
            return ApiResponse<RevenueReportDto>.Fail("Not implemented completely");
        }

        public async Task<ApiResponse<IEnumerable<OccupancyReportDto>>> GetOccupancyReportAsync(DateTime startDate, DateTime endDate)
        {
            return ApiResponse<IEnumerable<OccupancyReportDto>>.Fail("Not implemented completely");
        }
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly SaowariDbContext _context;
        public UserProfileService(SaowariDbContext context) { _context = context; }

        public async Task<ApiResponse<object>> GetMyBookingsAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Route)
                .Where(b => b.UserID == userId)
                .Select(b => new {
                    b.BookingID,
                    b.BookingCode,
                    b.BookingDate,
                    Route = b.Schedule.Route.FromLocation.LocationName + " to " + b.Schedule.Route.ToLocation.LocationName,
                    IsPast = b.Schedule.DepartureDateTime < DateTime.UtcNow
                })
                .ToListAsync();

            return ApiResponse<object>.Ok(bookings);
        }

        public async Task<ApiResponse<InvoiceDto>> GetBookingInvoiceAsync(int userId, int bookingId)
        {
            return ApiResponse<InvoiceDto>.Fail("Not implemented completely");
        }
    }

    public class DiscountValidationService : IDiscountValidationService
    {
        private readonly SaowariDbContext _context;
        public DiscountValidationService(SaowariDbContext context) { _context = context; }

        public async Task<ApiResponse<DiscountValidationDto>> ValidateDiscountAsync(int discountId, int scheduleId, decimal baseAmount)
        {
            return ApiResponse<DiscountValidationDto>.Fail("Not implemented completely");
        }
    }
}
