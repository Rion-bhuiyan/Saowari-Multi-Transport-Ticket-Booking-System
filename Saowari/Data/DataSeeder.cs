using Saowari.Data;
using Saowari.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Saowari.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(SaowariDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            var roles = new[] { "Admin", "Agent", "Customer", "Driver", "Supervisor" };
            bool rolesAdded = false;

            foreach (var roleName in roles)
            {
                if (!await context.UserRoles.AnyAsync(r => r.UserRoleName == roleName))
                {
                    context.UserRoles.Add(new UserRole { UserRoleName = roleName });
                    rolesAdded = true;
                }
            }

            if (rolesAdded)
            {
                await context.SaveChangesAsync();
            }

            // Seed Company Types
            var companyTypes = new[] { "Bus", "Train", "Flight", "Ship" };
            bool typesAdded = false;

            foreach (var typeName in companyTypes)
            {
                if (!await context.CompanyTypes.AnyAsync(ct => ct.CompanyTypeName == typeName))
                {
                    context.CompanyTypes.Add(new CompanyType { CompanyTypeName = typeName });
                    typesAdded = true;
                }
            }

            if (typesAdded)
            {
                await context.SaveChangesAsync();
            }

            // Seed Seat Statuses
            var seatStatuses = new[] { "Available", "Booked", "Reserved", "Blocked" };
            bool seatStatusAdded = false;

            foreach (var statusName in seatStatuses)
            {
                if (!await context.SeatStatuses.AnyAsync(s => s.StatusName == statusName))
                {
                    context.SeatStatuses.Add(new SeatStatus { StatusName = statusName });
                    seatStatusAdded = true;
                }
            }

            if (seatStatusAdded)
            {
                await context.SaveChangesAsync();
            }

            // Seed Schedule Statuses
            var scheduleStatuses = new[] { "Active", "Scheduled", "Completed", "Cancelled", "Delayed", "Pending Expiry", "Expired" };
            bool scheduleStatusAdded = false;

            foreach (var statusName in scheduleStatuses)
            {
                if (!await context.ScheduleStatuses.AnyAsync(s => s.ScheduleStatusName == statusName))
                {
                    context.ScheduleStatuses.Add(new ScheduleStatus { ScheduleStatusName = statusName });
                    scheduleStatusAdded = true;
                }
            }

            if (scheduleStatusAdded)
            {
                await context.SaveChangesAsync();
            }

            // Seed Booking Statuses
            var bookingStatusesList = new[] { "Pending", "Approved", "Cancelled", "Completed" };
            bool bookingStatusAdded = false;
            foreach (var statusName in bookingStatusesList)
            {
                if (!await context.BookingStatuses.AnyAsync(bs => bs.BookingStatusName == statusName))
                {
                    context.BookingStatuses.Add(new BookingStatus { BookingStatusName = statusName });
                    bookingStatusAdded = true;
                }
            }
            if (bookingStatusAdded)
            {
                await context.SaveChangesAsync();
            }

            // Seed Payment Statuses
            var paymentStatusesList = new[] { "Pending", "Paid", "Failed", "Refunded" };
            bool paymentStatusAdded = false;
            foreach (var statusName in paymentStatusesList)
            {
                if (!await context.paymentStatuses.AnyAsync(ps => ps.PaymentStatusName == statusName))
                {
                    context.paymentStatuses.Add(new PaymentStatus { PaymentStatusName = statusName });
                    paymentStatusAdded = true;
                }
            }
            if (paymentStatusAdded)
            {
                await context.SaveChangesAsync();
            }

            // Seed Refund Statuses
            var refundStatusesList = new[] { "Pending", "Approved", "Rejected", "Processed" };
            bool refundStatusAdded = false;
            foreach (var statusName in refundStatusesList)
            {
                if (!await context.RefundStatuses.AnyAsync(rs => rs.StatusName == statusName))
                {
                    context.RefundStatuses.Add(new RefundStatus { StatusName = statusName });
                    refundStatusAdded = true;
                }
            }
            if (refundStatusAdded)
            {
                await context.SaveChangesAsync();
            }

            // Seed Payment Methods with fee structures
            var paymentMethods = new[]
            {
                new { Name = "bKash",  ProcessingFee = 1.5m,  VAT = 15m },
                new { Name = "Nagad",  ProcessingFee = 1.0m,  VAT = 15m },
                new { Name = "Rocket", ProcessingFee = 1.2m,  VAT = 15m },
                new { Name = "Card",   ProcessingFee = 2.0m,  VAT = 15m },
                new { Name = "Cash",   ProcessingFee = 0.0m,  VAT = 0m  },
            };
            bool pmAdded = false;
            foreach (var pm in paymentMethods)
            {
                if (!await context.PaymentMethods.AnyAsync(m => m.PaymentMethodName == pm.Name))
                {
                    context.PaymentMethods.Add(new Saowari.Models.Entities.PaymentMethod
                    {
                        PaymentMethodName = pm.Name,
                        ProcessingFeePercent = pm.ProcessingFee,
                        VATPercent = pm.VAT,
                        IsActive = true
                    });
                    pmAdded = true;
                }
            }
            if (pmAdded) await context.SaveChangesAsync();

            // Seed Admin User
            if (!await context.Users.AnyAsync(u => u.Email == "admin@saowari.com"))
            {
                var adminRole = await context.UserRoles.FirstOrDefaultAsync(r => r.UserRoleName == "Admin");
                if (adminRole != null)
                {
                    var admin = new User
                    {
                        FullName = "System Administrator",
                        Email = "admin@saowari.com",
                        Phone = "01700000000",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                        RoleID = adminRole.UserRoleId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(admin);
                    await context.SaveChangesAsync();
                }
            }

            // Fix existing schedules with 0 base price
            var zeroPriceSchedules = await context.Schedules
                .Include(s => s.ScheduleSeatClassPricings)
                .Where(s => s.BasePrice == 0)
                .ToListAsync();

            if (zeroPriceSchedules.Any())
            {
                foreach (var schedule in zeroPriceSchedules)
                {
                    var customPricing = schedule.ScheduleSeatClassPricings?.FirstOrDefault()?.Price;
                    schedule.BasePrice = customPricing > 0 ? customPricing.Value : 1200m;
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
