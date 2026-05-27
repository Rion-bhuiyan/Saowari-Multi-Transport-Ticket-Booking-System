using AutoMapper;
using Microsoft.AspNetCore.Http;
using Saowari.Data;
using Saowari.Interfaces;
using Saowari.Models.DTOs.Booking;
using Saowari.Models.Entities;
using Saowari.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Saowari.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _repository;
        private readonly IMapper _mapper;
        private readonly SaowariDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public BookingService(IRepository<Booking> repository, IMapper mapper, SaowariDbContext context, IHttpContextAccessor httpContextAccessor, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, INotificationService notificationService, IEmailService emailService)
        {
            _repository = repository;
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<ApiResponse<IEnumerable<BookingResponseDto>>> GetAllAsync()
        {
            var entities = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.FromLocation)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.ToLocation)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Vehicle)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<BookingResponseDto>>(entities);
            return ApiResponse<IEnumerable<BookingResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<IEnumerable<BookingResponseDto>>> GetMyAsync(int userId)
        {
            var entities = await _context.Bookings
                .Include(b => b.BookingStatus)
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.FromLocation)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.ToLocation)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Vehicle)
                .Include(b => b.Refunds)
                .Where(b => b.UserID == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            
            var dtos = _mapper.Map<IEnumerable<BookingResponseDto>>(entities).ToList();
            
            // Set additional flags
            var now = DateTime.UtcNow;
            for (int i = 0; i < dtos.Count; i++)
            {
                var entity = entities[i];
                if (!string.IsNullOrEmpty(entity.CancellationOtp) && entity.CancellationOtpExpiry > now)
                    dtos[i].HasPendingCancellation = true;

                var latestRefund = entity.Refunds.OrderByDescending(r => r.RequestedAt).FirstOrDefault();
                if (latestRefund != null)
                {
                    dtos[i].LatestRefundId = latestRefund.RefundID;
                    dtos[i].LatestRefundStatusId = latestRefund.RefundStatusId;
                }
            }
            
            return ApiResponse<IEnumerable<BookingResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<BookingResponseDto>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<BookingResponseDto>.Fail("Not found");
            return ApiResponse<BookingResponseDto>.Ok(_mapper.Map<BookingResponseDto>(entity));
        }

        public async Task<ApiResponse<TicketDetailsDto>> GetTicketDetailsAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.FromLocation)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Route)
                        .ThenInclude(r => r.ToLocation)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Vehicle)
                        .ThenInclude(v => v.Company)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Supervisor)
                .Include(b => b.Schedule)
                    .ThenInclude(s => s.Supervisor)
                .Include(b => b.BookingStatus)
                .Include(b => b.Discount)
                .Include(b => b.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return ApiResponse<TicketDetailsDto>.Fail("Booking not found");

            if (booking.BookingStatus != null && 
                (booking.BookingStatus.BookingStatusName == "Cancelled" || booking.BookingStatus.BookingStatusName == "Refunded"))
            {
                return ApiResponse<TicketDetailsDto>.Fail("This ticket has been cancelled or refunded and is no longer valid.");
            }

            var payment = booking.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

            // Build base URL for static assets
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}{request.PathBase}" : "";

            // Get supervisor name & phone from the User table
            var supervisorName = "N/A";
            var supervisorPhone = "N/A";
            if (booking.Schedule?.SupervisorId != null)
            {
                var supervisorUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.SupervisorId == booking.Schedule.SupervisorId);
                if (supervisorUser != null)
                {
                    supervisorName = supervisorUser.FullName;
                    supervisorPhone = supervisorUser.Phone;
                }
            }

            // Build image URLs
            var saowariLogoUrl = $"{baseUrl}/uploads/site/logo.png";

            // Ticket background image (admin-configurable)
            var ticketBgPath = System.IO.Path.Combine(
                _env.WebRootPath ?? System.IO.Path.Combine(_env.ContentRootPath, "wwwroot"),
                "uploads", "site", "ticket-background.jpg");
            
            var companyTicketBgUrl = booking.Schedule?.Vehicle?.Company?.TicketBackgroundUrl;
            var ticketBackgroundUrl = !string.IsNullOrEmpty(companyTicketBgUrl) 
                ? (companyTicketBgUrl.StartsWith("http") ? companyTicketBgUrl : $"{baseUrl}/{companyTicketBgUrl.TrimStart('/')}")
                : (System.IO.File.Exists(ticketBgPath) ? $"{baseUrl}/uploads/site/ticket-background.jpg" : null);

            decimal ticketBackgroundOpacity = 0.1m;
            if (!string.IsNullOrEmpty(companyTicketBgUrl))
            {
                ticketBackgroundOpacity = booking.Schedule?.Vehicle?.Company?.TicketBackgroundOpacity ?? 0.1m;
            }
            else
            {
                var globalOpacitySetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "TicketBackgroundOpacity");
                if (globalOpacitySetting != null && decimal.TryParse(globalOpacitySetting.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal globalOpacity))
                {
                    ticketBackgroundOpacity = globalOpacity;
                }
            }

            var companyLogoUrl = booking.Schedule?.Vehicle?.Company?.LogoURL ?? "";
            // If company logo is a relative path, prepend base URL
            if (!string.IsNullOrEmpty(companyLogoUrl) && !companyLogoUrl.StartsWith("http"))
                companyLogoUrl = $"{baseUrl}/{companyLogoUrl.TrimStart('/')}";

            var routeImageUrl = booking.Schedule?.Route?.ImageUrl ?? "";
            if (!string.IsNullOrEmpty(routeImageUrl) && !routeImageUrl.StartsWith("http"))
                routeImageUrl = $"{baseUrl}/{routeImageUrl.TrimStart('/')}";

            var seatNumbers = booking.BookingSeats.Select(bs => bs.Seat.SeatNumber).ToList();
            int seatCount = seatNumbers.Count < 1 ? 1 : seatNumbers.Count;
            decimal pricePerSeat = seatCount > 0 ? Math.Round(booking.BaseAmount / seatCount, 2) : booking.BaseAmount;

            var pricings = await _context.ScheduleSeatClassPricings
                .Where(p => p.ScheduleId == booking.ScheduleID)
                .ToDictionaryAsync(p => p.SeatClassId, p => p.Price);

            var seatDetails = booking.BookingSeats.Select(bs => new SeatDetailDto
            {
                SeatNumber = bs.Seat.SeatNumber,
                Price = pricings.TryGetValue(bs.Seat.SeatClassId, out var customPrice) ? customPrice : booking.Schedule?.BasePrice ?? 0
            }).ToList();

            var dto = new TicketDetailsDto
            {
                BookingID = booking.BookingID,
                BookingCode = booking.BookingCode,
                PassengerName = booking.PassengerName,
                PassengerPhone = booking.PassengerPhone,
                PassengerNID = booking.PassengerNID,
                BookingDate = booking.BookingDate,
                BaseAmount = booking.BaseAmount,
                DiscountAmount = booking.DiscountAmount,
                ProcessingFeeAmount = booking.ProcessingFeeAmount,
                VATAmount = booking.VATAmount,
                FinalAmount = booking.FinalAmount,
                SeatCount = seatCount,
                PricePerSeat = pricePerSeat,
                CouponCode = booking.Discount?.CouponCode,
                DiscountName = booking.Discount?.DiscountName,
                IsPercentageDiscount = booking.Discount?.DiscountTypeId == 2 || (booking.Discount?.DiscountType?.DiscountTypeName?.Contains("Percent", StringComparison.OrdinalIgnoreCase) ?? false),
                DiscountValue = booking.Discount?.DiscountValue ?? 0,
                Status = booking.BookingStatus?.BookingStatusName ?? "Unknown",
                SeatDetails = seatDetails,

                SaowariLogoUrl = saowariLogoUrl,
                TicketBackgroundUrl = ticketBackgroundUrl,
                TicketBackgroundOpacity = ticketBackgroundOpacity,
                CompanyName = booking.Schedule?.Vehicle?.Company?.CompanyName ?? "Unknown Company",
                CompanyLogoUrl = companyLogoUrl,
                VehicleName = booking.Schedule?.Vehicle?.VehicleName ?? "Unknown Vehicle",
                VehicleRegNumber = booking.Schedule?.Vehicle?.VehicleNumber ?? "Unknown",
                VehicleTypeName = booking.Schedule?.Vehicle?.VehicleType?.VehicleTypeName ?? "Unknown",
                IsAc = false,

                SupervisorName = supervisorName,
                SupervisorPhone = supervisorPhone,

                FromLocation = booking.Schedule?.Route?.FromLocation?.LocationName ?? "Unknown",
                ToLocation = booking.Schedule?.Route?.ToLocation?.LocationName ?? "Unknown",
                BoardingPoint = booking.BoardingPoint,
                RouteImageUrl = routeImageUrl,
                DepartureTime = booking.Schedule?.DepartureDateTime ?? DateTime.MinValue,
                ArrivalTime = booking.Schedule?.ArrivalDateTime ?? DateTime.MinValue,

                PaymentMethod = payment?.PaymentMethod?.PaymentMethodName,
                TransactionId = payment?.TransactionID,
                PaidAt = payment?.PaidAt,

                SeatNumbers = seatNumbers
            };

            return ApiResponse<TicketDetailsDto>.Ok(dto);
        }

        public async Task<ApiResponse<TicketDetailsDto>> GetTicketDetailsByCodeAsync(string bookingCode)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);

            if (booking == null) return ApiResponse<TicketDetailsDto>.Fail("Booking not found");

            return await GetTicketDetailsAsync(booking.BookingID);
        }

        public async Task<ApiResponse<BookingResponseDto>> CreateAsync(BookingCreateDto dto)
        {
            // 1. Validate Schedule & Seats
            var schedule = await _context.Schedules.FindAsync(dto.ScheduleID);
            if (schedule == null) return ApiResponse<BookingResponseDto>.Fail("Schedule not found");

            var requestedSeatIds = dto.SeatIds.Any() ? dto.SeatIds : dto.Passengers.Select(p => p.SeatId).Distinct().ToList();
            if (!requestedSeatIds.Any()) return ApiResponse<BookingResponseDto>.Fail("No seats selected for booking");

            var seatStatuses = await _context.ScheduleSeatStatuses
                .Include(s => s.SeatStatus)
                .Where(s => s.ScheduleID == dto.ScheduleID && requestedSeatIds.Contains(s.SeatID))
                .ToListAsync();

            if (seatStatuses.Any(s => s.SeatStatus.StatusName != "Available"))
            {
                return ApiResponse<BookingResponseDto>.Fail("One or more selected seats are no longer available");
            }

            // 2. Determine Pricing & Amounts
            var pricings = await _context.ScheduleSeatClassPricings
                .Where(p => p.ScheduleId == dto.ScheduleID)
                .ToDictionaryAsync(p => p.SeatClassId, p => p.Price);

            var seats = await _context.Seats
                .Where(s => requestedSeatIds.Contains(s.SeatID))
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

            if (dto.DiscountID.HasValue)
            {
                var discount = await _context.Discounts
                    .Include(d => d.DiscountType)
                    .FirstOrDefaultAsync(d => d.DiscountID == dto.DiscountID.Value);

                if (discount != null && discount.IsActive && discount.StartDate <= DateTime.UtcNow && discount.EndDate >= DateTime.UtcNow)
                {
                    bool isPercentage = discount.DiscountType != null && discount.DiscountType.DiscountTypeName.Contains("Percent", StringComparison.OrdinalIgnoreCase);
                    
                    if (isPercentage || discount.DiscountTypeId == 2)
                    {
                        discountAmount = (baseAmount * discount.DiscountValue) / 100m;
                    }
                    else
                    {
                        discountAmount = discount.DiscountValue;
                    }
                    
                    // Cap discount at base amount
                    if (discountAmount > baseAmount)
                    {
                        discountAmount = baseAmount;
                    }
                }
            }

            decimal finalAmount = baseAmount - discountAmount;

            // 2b. Resolve payment method fee & VAT
            decimal processingFeeAmount = 0;
            decimal vatAmount = 0;

            if (!string.IsNullOrEmpty(dto.PaymentMethod))
            {
                var pm = await _context.PaymentMethods
                    .FirstOrDefaultAsync(p => p.PaymentMethodName == dto.PaymentMethod && p.IsActive)
                    ?? await _context.PaymentMethods.FirstOrDefaultAsync(p => p.IsActive)
                    ?? await _context.PaymentMethods.FirstOrDefaultAsync();

                if (pm != null && pm.ProcessingFeePercent > 0)
                {
                    processingFeeAmount = Math.Round(finalAmount * pm.ProcessingFeePercent / 100m, 2);
                    if (pm.VATPercent > 0)
                        vatAmount = Math.Round(baseAmount * pm.VATPercent / 100m, 2);
                }
            }

            decimal grandTotal = finalAmount + processingFeeAmount + vatAmount;

            // 3. Get Status Lookups
            var confirmedStatus = await _context.BookingStatuses.FirstOrDefaultAsync(b => b.BookingStatusName == "Confirmed");
            var bookedSeatStatus = await _context.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Booked");
            var defaultSeatClass = await _context.SeatClasses.FirstOrDefaultAsync();

            int bookingStatusId = dto.BookingStatusId ?? confirmedStatus?.BookingStatusId ?? 1;
            int seatClassId = dto.SeatClassId > 0 ? dto.SeatClassId : defaultSeatClass?.SeatClassId ?? 1;

            // 4. Create Booking Entity
            var booking = new Booking
            {
                BookingCode = string.IsNullOrEmpty(dto.BookingCode) ? $"B{DateTime.UtcNow:yyMMddHHmmss}{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}" : dto.BookingCode,
                UserID = dto.UserID > 0 ? dto.UserID : 1,
                ScheduleID = dto.ScheduleID,
                PassengerName = string.IsNullOrEmpty(dto.PassengerName) ? dto.Passengers.FirstOrDefault()?.PassengerName ?? "Guest Passenger" : dto.PassengerName,
                PassengerPhone = string.IsNullOrEmpty(dto.PassengerPhone) ? dto.Passengers.FirstOrDefault()?.MobileNumber ?? dto.MobileNumber ?? "01000000000" : dto.PassengerPhone,
                PassengerNID = dto.PassengerNID,
                BaseAmount = baseAmount,
                DiscountAmount = discountAmount,
                ProcessingFeeAmount = processingFeeAmount,
                VATAmount = vatAmount,
                FinalAmount = grandTotal,
                DiscountID = dto.DiscountID,
                BookingStatusId = bookingStatusId,
                BookingDate = DateTime.UtcNow,
                SeatClassId = seatClassId,
                BoardingPoint = dto.BoardingPoint
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync(); // Save to get BookingID

            // 5. Link BookingSeats & Update ScheduleSeatStatus
            foreach (var seatId in requestedSeatIds)
            {
                await _context.BookingSeats.AddAsync(new BookingSeat
                {
                    BookingId = booking.BookingID,
                    SeatId = seatId
                });

                var statusRecord = seatStatuses.FirstOrDefault(s => s.SeatID == seatId);
                if (statusRecord != null && bookedSeatStatus != null)
                {
                    statusRecord.SeatStatusId = bookedSeatStatus.SeatStatusId;
                    statusRecord.BookingID = booking.BookingID;
                    _context.ScheduleSeatStatuses.Update(statusRecord);
                }
            }

            // 6. Update Schedule Available Seats Count
            schedule.AvailableSeats = Math.Max(0, schedule.AvailableSeats - requestedSeatIds.Count);
            _context.Schedules.Update(schedule);

            // 7. Create Payment Record if payment details provided
            if (!string.IsNullOrEmpty(dto.PaymentMethod) || !string.IsNullOrEmpty(dto.TransactionId))
            {
                var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(p => p.PaymentMethodName == dto.PaymentMethod)
                                 ?? await _context.PaymentMethods.FirstOrDefaultAsync();
                
                var completedPayStatus = await _context.paymentStatuses.FirstOrDefaultAsync(p => p.PaymentStatusName == "Completed")
                                      ?? await _context.paymentStatuses.FirstOrDefaultAsync();

                var payment = new Payment
                {
                    BookingId = booking.BookingID,
                    Amount = grandTotal,
                    DiscountAmount = discountAmount,
                    ProcessingFeeAmount = processingFeeAmount,
                    VATAmount = vatAmount,
                    PaymentMethodId = paymentMethod?.PaymentMethodId ?? 1,
                    TransactionID = string.IsNullOrEmpty(dto.TransactionId) ? $"TXN-{booking.BookingID}-{new Random().Next(1000, 9999)}" : dto.TransactionId,
                    PaymentStatusId = completedPayStatus?.PaymentStatusId ?? 1,
                    PaidAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
            }

            await _context.SaveChangesAsync();

            try
            {
                await _notificationService.NotifyBookingCreatedAsync(booking);
            }
            catch (Exception) { /* Fail-safe */ }

            var responseDto = _mapper.Map<BookingResponseDto>(booking);
            return ApiResponse<BookingResponseDto>.Ok(responseDto, "Booking confirmed successfully");
        }

        public async Task<ApiResponse<BookingResponseDto>> UpdateAsync(int id, BookingUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<BookingResponseDto>.Fail("Not found");

            var cancelledStatus = await _context.BookingStatuses.FirstOrDefaultAsync(s => s.BookingStatusName == "Cancelled");
            bool isBecomingCancelled = false;
            if (cancelledStatus != null && dto.BookingStatusId == cancelledStatus.BookingStatusId && entity.BookingStatusId != cancelledStatus.BookingStatusId)
            {
                isBecomingCancelled = true;
            }
            
            _mapper.Map(dto, entity);
            _repository.Update(entity);
            await _repository.SaveAsync();

            if (isBecomingCancelled)
            {
                var fullEntity = await _context.Bookings
                    .Include(b => b.BookingSeats)
                    .Include(b => b.Schedule)
                    .FirstOrDefaultAsync(b => b.BookingID == id);

                if (fullEntity != null)
                {
                    var availableSeatStatus = await _context.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
                    if (availableSeatStatus != null && fullEntity.Schedule != null)
                    {
                        var seatIds = fullEntity.BookingSeats.Select(bs => bs.SeatId).ToList();
                        var scheduleSeatStatuses = await _context.ScheduleSeatStatuses
                            .Where(s => s.ScheduleID == fullEntity.ScheduleID && seatIds.Contains(s.SeatID))
                            .ToListAsync();

                        foreach (var status in scheduleSeatStatuses)
                        {
                            status.SeatStatusId = availableSeatStatus.SeatStatusId;
                            _context.ScheduleSeatStatuses.Update(status);
                        }
                        
                        fullEntity.Schedule.AvailableSeats += seatIds.Count;
                        _context.Schedules.Update(fullEntity.Schedule);
                        await _context.SaveChangesAsync();
                    }
                }

                try
                {
                    await _notificationService.NotifyBookingCancelledAsync(entity);
                }
                catch (Exception) { /* Fail-safe */ }
            }
            
            return ApiResponse<BookingResponseDto>.Ok(_mapper.Map<BookingResponseDto>(entity), "Updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Fail("Not found");
            
            _repository.Remove(entity);
            await _repository.SaveAsync();
            
            return ApiResponse<bool>.Ok(true, "Deleted successfully");
        }

        public async Task<ApiResponse<bool>> RequestCancellationAsync(int id)
        {
            Console.WriteLine($"RequestCancellationAsync called for BookingID: {id}");
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) 
            {
                Console.WriteLine("Booking not found");
                return ApiResponse<bool>.Fail("Booking not found");
            }

            Console.WriteLine($"Booking found. UserID: {booking.UserID}, User object is null: {booking.User == null}");

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            booking.CancellationOtp = otp;
            booking.CancellationOtpExpiry = DateTime.UtcNow.AddMinutes(15);

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"OTP generated and saved: {otp}");

            // Notify user in real-time
            try 
            {
                await _notificationService.NotifyBookingCancellationOtpAsync(booking, otp);
                Console.WriteLine("Real-time OTP notification sent to user.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send real-time notification: {ex.Message}");
            }

            // Send Email to User
            if (booking.User != null && !string.IsNullOrEmpty(booking.User.Email))
            {
                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2>Booking Cancellation Request</h2>
                        <p>Hello {booking.User.FullName},</p>
                        <p>A request was made to cancel your booking <b>#{booking.BookingCode}</b>.</p>
                        <p>To confirm this cancellation, please provide the following OTP to the admin:</p>
                        <h1 style='color: #d9534f; letter-spacing: 2px;'>{otp}</h1>
                        <p>This OTP is valid for 15 minutes.</p>
                    </div>";
                var plainBody = $"Your cancellation OTP for booking {booking.BookingCode} is {otp}. Valid for 15 minutes.";

                try 
                {
                    Console.WriteLine($"Attempting to send email to {booking.User.Email}");
                    await _emailService.SendEmailAsync(booking.User.Email, $"Cancellation OTP - {booking.BookingCode}", htmlBody, plainBody);
                    Console.WriteLine("Email sent successfully.");
                } catch (Exception ex) { 
                    Console.WriteLine($"Failed to send email: {ex.Message}\n{ex.StackTrace}");
                }
            }

            // Create Notification for User
            if (booking.User != null)
            {
                var notification = new Notification
                {
                    UserId = booking.User.UserID,
                    Title = "Cancellation OTP",
                    Message = $"Your cancellation OTP for booking {booking.BookingCode} is {otp}.",
                    Type = "cancellation",
                    EntityType = "Booking",
                    EntityId = booking.BookingID,
                    Icon = "fas fa-shield-alt",
                    ColorClass = "bg-red-100 text-red-600",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                await _context.Set<Notification>().AddAsync(notification);
                await _context.SaveChangesAsync();
                Console.WriteLine("Notification created successfully.");
            }

            return ApiResponse<bool>.Ok(true, "OTP sent successfully");
        }

        public async Task<ApiResponse<bool>> VerifyCancellationAsync(int id, string otp)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .Include(b => b.Schedule)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return ApiResponse<bool>.Fail("Booking not found");

            if (string.IsNullOrEmpty(booking.CancellationOtp) || booking.CancellationOtp != otp)
            {
                return ApiResponse<bool>.Fail("Invalid OTP");
            }

            if (booking.CancellationOtpExpiry < DateTime.UtcNow)
            {
                return ApiResponse<bool>.Fail("OTP has expired");
            }

            // Valid OTP -> Proceed to cancel
            var cancelledStatus = await _context.BookingStatuses.FirstOrDefaultAsync(s => s.BookingStatusName == "Cancelled");
            if (cancelledStatus != null)
            {
                booking.BookingStatusId = cancelledStatus.BookingStatusId;
            }
            booking.CancelReason = "Cancelled via OTP verification";

            // Release seats
            var availableSeatStatus = await _context.SeatStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
            if (availableSeatStatus != null && booking.Schedule != null)
            {
                var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
                var scheduleSeatStatuses = await _context.ScheduleSeatStatuses
                    .Where(s => s.ScheduleID == booking.ScheduleID && seatIds.Contains(s.SeatID))
                    .ToListAsync();

                foreach (var status in scheduleSeatStatuses)
                {
                    status.SeatStatusId = availableSeatStatus.SeatStatusId;
                    status.BookingID = null;
                    _context.ScheduleSeatStatuses.Update(status);
                }

                // Restore available seats count
                booking.Schedule.AvailableSeats += seatIds.Count;
                _context.Schedules.Update(booking.Schedule);
            }

            // Clear OTP
            booking.CancellationOtp = null;
            booking.CancellationOtpExpiry = null;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            // Notify user of successful cancellation
            try
            {
                await _notificationService.NotifyBookingCancelledAsync(booking);
            }
            catch (Exception) { /* Fail-safe */ }

            return ApiResponse<bool>.Ok(true, "Booking cancelled successfully");
        }
    }
}