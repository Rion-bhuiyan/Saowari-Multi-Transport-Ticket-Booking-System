using Microsoft.EntityFrameworkCore;
using Saowari.Models.Entities;
using Route = Saowari.Models.Entities.Route;

namespace Saowari.Data
{
    public class SaowariDbContext : DbContext
    {
        public SaowariDbContext(DbContextOptions<SaowariDbContext> options) : base(options)
        {
        }

        // Core Booking
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingSeat> BookingSeats { get; set; }
        public DbSet<BookingStatus> BookingStatuses { get; set; }

        // Company
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyType> CompanyTypes { get; set; }

        // Discount
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountType> DiscountTypes { get; set; }

        // Location & Route
        public DbSet<Location> Locations { get; set; }
        public DbSet<Models.Entities.Route> Routes { get; set; }
        public DbSet<DepartureLocation> DepartureLocations { get; set; }
        public DbSet<PaymentStatus> paymentStatuses { get; set; }

        // Payment
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentCancel> PaymentCancels { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        // Refund
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<RefundPolicy> RefundPolicies { get; set; }
        public DbSet<RefundStatus> RefundStatuses { get; set; }

        // Schedule
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleSeatStatus> ScheduleSeatStatuses { get; set; }
        public DbSet<ScheduleStatus> ScheduleStatuses { get; set; }

        // Seat
        public DbSet<Seat> Seats { get; set; }
        public DbSet<SeatClass> SeatClasses { get; set; }
        public DbSet<SeatPricing> SeatPricings { get; set; }
        public DbSet<SeatStatus> SeatStatuses { get; set; }
        public DbSet<ScheduleSeatClassPricing> ScheduleSeatClassPricings { get; set; }

        // Ticket
        public DbSet<Ticket> Tickets { get; set; }

        // User
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminNotificationPreference> AdminNotificationPreferences { get; set; }
        public DbSet<Banner> Banners { get; set; } = null!;
        
        // Chat Messaging System
        public DbSet<SupportRoom> SupportRooms { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
        public DbSet<ScheduleChatMessage> ScheduleChatMessages { get; set; }

        // Driver
        public DbSet<DriverInformtion> DriverInformtions { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }

        // Vehicle
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }

        // Home Slider
        public DbSet<SliderImage> SliderImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================
            // LOCATION & ROUTE
            // =====================

            modelBuilder.Entity<Route>()
                .HasOne(r => r.FromLocation)
                .WithMany(l => l.DepartureRoutes)
                .HasForeignKey(r => r.FromLocationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Route>()
                .HasOne(r => r.ToLocation)
                .WithMany(l => l.ArrivalRoutes)
                .HasForeignKey(r => r.ToLocationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DepartureLocation>()
                .HasOne(dl => dl.Schedule)
                .WithMany(s => s.DepartureLocations)
                .HasForeignKey(dl => dl.ScheduleID)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // COMPANY
            // =====================

            modelBuilder.Entity<Company>()
                .HasOne(c => c.CompanyType)
                .WithMany(ct => ct.Companies)
                .HasForeignKey(c => c.CompanyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // VEHICLE
            // =====================

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Company)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.VehicleType)
                .WithMany(vt => vt.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // SEAT
            // =====================

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.Seats)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.SeatClass)
                .WithMany(sc => sc.Seats)
                .HasForeignKey(s => s.SeatClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // SEAT PRICING (per vehicle)
            // =====================

            modelBuilder.Entity<SeatPricing>()
                .HasOne(sp => sp.Vehicle)
                .WithMany(v => v.SeatPricings)
                .HasForeignKey(sp => sp.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeatPricing>()
                .HasOne(sp => sp.SeatClass)
                .WithMany(sc => sc.SeatPricings)
                .HasForeignKey(sp => sp.SeatClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SeatPricing>()
                .HasIndex(sp => new { sp.VehicleId, sp.SeatClassId })
                .IsUnique();

            // =====================
            // SCHEDULE SEAT CLASS PRICING (per schedule override)
            // =====================

            modelBuilder.Entity<ScheduleSeatClassPricing>()
                .HasOne(p => p.Schedule)
                .WithMany(s => s.ScheduleSeatClassPricings)
                .HasForeignKey(p => p.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ScheduleSeatClassPricing>()
                .HasOne(p => p.SeatClass)
                .WithMany()
                .HasForeignKey(p => p.SeatClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleSeatClassPricing>()
                .HasIndex(p => new { p.ScheduleId, p.SeatClassId })
                .IsUnique();

            // =====================
            // SCHEDULE
            // =====================

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Schedules)
                .HasForeignKey(s => s.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.Schedules)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.DriverInformtion)
                .WithMany(d => d.Schedules)
                .HasForeignKey(s => s.DriverInformtionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Supervisor)
                .WithMany(su => su.Schedules)
                .HasForeignKey(s => s.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.ScheduleStatus)
                .WithMany(ss => ss.Schedules)
                .HasForeignKey(s => s.ScheduleStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // BOOKING
            // =====================

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Schedule)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ScheduleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.BookingStatus)
                .WithMany(bs => bs.Bookings)
                .HasForeignKey(b => b.BookingStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Discount)
                .WithMany(d => d.Bookings)
                .HasForeignKey(b => b.DiscountID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.SeatClass)
                .WithMany(sc => sc.Bookings)
                .HasForeignKey(b => b.SeatClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // BOOKING SEAT
            // =====================

            modelBuilder.Entity<BookingSeat>()
                .HasIndex(bs => new { bs.BookingId, bs.SeatId })
                .IsUnique();

            modelBuilder.Entity<BookingSeat>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingSeats)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat)
                .WithMany(s => s.BookingSeats)
                .HasForeignKey(bs => bs.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // SCHEDULE SEAT STATUS
            // =====================

            modelBuilder.Entity<ScheduleSeatStatus>()
                .HasIndex(ss => new { ss.ScheduleID, ss.SeatID })
                .IsUnique();

            modelBuilder.Entity<ScheduleSeatStatus>()
                .HasOne(ss => ss.Schedule)
                .WithMany(s => s.ScheduleSeatStatuses)
                .HasForeignKey(ss => ss.ScheduleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleSeatStatus>()
                .HasOne(ss => ss.Seat)
                .WithMany(s => s.ScheduleSeatStatuses)
                .HasForeignKey(ss => ss.SeatID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleSeatStatus>()
                .HasOne(ss => ss.Booking)
                .WithMany(b => b.ScheduleSeatStatuses)
                .HasForeignKey(ss => ss.BookingID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleSeatStatus>()
                .HasOne(ss => ss.SeatStatus)
                .WithMany(s => s.ScheduleSeatStatuses)
                .HasForeignKey(ss => ss.SeatStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // PAYMENT
            // =====================

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany(pm => pm.Payments)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentStatus)
                .WithMany(ps => ps.Payments)
                .HasForeignKey(p => p.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // PAYMENT CANCEL
            // =====================

            modelBuilder.Entity<PaymentCancel>()
                .HasOne(pc => pc.Payment)
                .WithMany(p => p.PaymentCancels)
                .HasForeignKey(pc => pc.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // REFUND
            // =====================

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Booking)
                .WithMany(b => b.Refunds)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Payment)
                .WithMany(p => p.Refunds)
                .HasForeignKey(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.RefundStatus)
                .WithMany(rs => rs.Refunds)
                .HasForeignKey(r => r.RefundStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Refund>()
                .HasOne(r => r.RefundPolicy)
                .WithMany(rp => rp.Refunds)
                .HasForeignKey(r => r.PolicyID)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // REFUND POLICY
            // =====================

            modelBuilder.Entity<RefundPolicy>()
                .HasOne(rp => rp.Company)
                .WithMany(c => c.RefundPolicies)
                .HasForeignKey(rp => rp.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // DISCOUNT
            // =====================

            modelBuilder.Entity<Discount>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Discounts)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Discount>()
                .HasOne(d => d.DiscountType)
                .WithMany(dt => dt.Discounts)
                .HasForeignKey(d => d.DiscountTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Discount>()
                .HasOne(d => d.Route)
                .WithMany(r => r.Discounts)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Discount>()
                .HasOne(d => d.VehicleType)
                .WithMany(vt => vt.Discounts)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // USER
            // =====================

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRole)
                .WithMany(ur => ur.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.DriverInformtion)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DriverInformtionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Supervisor)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // TICKET
            // =====================

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.Tickets)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // NOTIFICATION
            // =====================

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Company)
                .WithMany()
                .HasForeignKey(n => n.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // ADMIN NOTIFICATION PREFERENCE
            // =====================

            modelBuilder.Entity<AdminNotificationPreference>()
                .HasIndex(p => new { p.AdminUserId, p.CompanyId })
                .IsUnique();

            modelBuilder.Entity<AdminNotificationPreference>()
                .HasOne(p => p.AdminUser)
                .WithMany()
                .HasForeignKey(p => p.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdminNotificationPreference>()
                .HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // SUPPORT ROOM & SUPPORT MESSAGE
            // =====================

            modelBuilder.Entity<SupportRoom>()
                .HasOne(sr => sr.AssignedAdmin)
                .WithMany()
                .HasForeignKey(sr => sr.AssignedAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportMessage>()
                .HasOne(sm => sm.SupportRoom)
                .WithMany(sr => sr.SupportMessages)
                .HasForeignKey(sm => sm.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportMessage>()
                .HasOne(sm => sm.Sender)
                .WithMany()
                .HasForeignKey(sm => sm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // SCHEDULE CHAT MESSAGE
            // =====================

            modelBuilder.Entity<ScheduleChatMessage>()
                .HasOne(sc => sc.Schedule)
                .WithMany()
                .HasForeignKey(sc => sc.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ScheduleChatMessage>()
                .HasOne(sc => sc.Sender)
                .WithMany()
                .HasForeignKey(sc => sc.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

