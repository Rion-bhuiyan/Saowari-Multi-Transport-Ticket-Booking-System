using AutoMapper;
using Saowari.Models.Entities;
using Saowari.Models.DTOs.Booking;
using Saowari.Models.DTOs.BookingSeat;
using Saowari.Models.DTOs.BookingStatus;
using Saowari.Models.DTOs.Company;
using Saowari.Models.DTOs.CompanyType;
using Saowari.Models.DTOs.Discount;
using Saowari.Models.DTOs.DiscountType;
using Saowari.Models.DTOs.DriverInformtion;
using Saowari.Models.DTOs.Location;
using Saowari.Models.DTOs.Payment;
using Saowari.Models.DTOs.PaymentCancel;
using Saowari.Models.DTOs.PaymentMethod;
using Saowari.Models.DTOs.PaymentStatus;
using Saowari.Models.DTOs.Refund;
using Saowari.Models.DTOs.RefundPolicy;
using Saowari.Models.DTOs.RefundStatus;
using Saowari.Models.DTOs.Route;
using Saowari.Models.DTOs.Schedule;
using Saowari.Models.DTOs.ScheduleSeatStatus;
using Saowari.Models.DTOs.ScheduleStatus;
using Saowari.Models.DTOs.Seat;
using Saowari.Models.DTOs.SeatClass;
using Saowari.Models.DTOs.SeatPricing;
using Saowari.Models.DTOs.SeatStatus;
using Saowari.Models.DTOs.Supervisor;
using Saowari.Models.DTOs.Ticket;
using Saowari.Models.DTOs.User;
using Saowari.Models.DTOs.UserRole;
using Saowari.Models.DTOs.Vehicle;
using Saowari.Models.DTOs.VehicleType;
using Saowari.Models.DTOs.SliderImage;

namespace Saowari.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Booking, BookingCreateDto>().ReverseMap();
            CreateMap<Booking, BookingUpdateDto>().ReverseMap();
            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src => src.BookingStatus != null ? src.BookingStatus.BookingStatusName : null))
                .ForMember(dest => dest.DepartureDateTime, opt => opt.MapFrom(src => src.Schedule != null ? (DateTime?)src.Schedule.DepartureDateTime : null))
                .ForMember(dest => dest.FromLocation, opt => opt.MapFrom(src => src.Schedule != null && src.Schedule.Route != null && src.Schedule.Route.FromLocation != null ? src.Schedule.Route.FromLocation.LocationName : null))
                .ForMember(dest => dest.ToLocation, opt => opt.MapFrom(src => src.Schedule != null && src.Schedule.Route != null && src.Schedule.Route.ToLocation != null ? src.Schedule.Route.ToLocation.LocationName : null))
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src => src.Schedule != null && src.Schedule.Vehicle != null ? src.Schedule.Vehicle.VehicleName : null))
                .ForMember(dest => dest.NumberOfSeats, opt => opt.MapFrom(src => src.BookingSeats != null ? src.BookingSeats.Count : 0))
                .ForMember(dest => dest.SeatNumbers, opt => opt.MapFrom(src => src.BookingSeats != null ? string.Join(", ", src.BookingSeats.Where(bs => bs.Seat != null).Select(bs => bs.Seat!.SeatNumber)) : null));
            CreateMap<BookingResponseDto, Booking>();

            CreateMap<BookingSeat, BookingSeatCreateDto>().ReverseMap();
            CreateMap<BookingSeat, BookingSeatUpdateDto>().ReverseMap();
            CreateMap<BookingSeat, BookingSeatResponseDto>().ReverseMap();

            CreateMap<BookingStatus, BookingStatusCreateDto>().ReverseMap();
            CreateMap<BookingStatus, BookingStatusUpdateDto>().ReverseMap();
            CreateMap<BookingStatus, BookingStatusResponseDto>().ReverseMap();

            CreateMap<Company, CompanyCreateDto>().ReverseMap();
            CreateMap<CompanyUpdateDto, Company>().ForMember(dest => dest.CompanyID, opt => opt.Ignore());
            CreateMap<Company, CompanyUpdateDto>();
            CreateMap<Company, CompanyResponseDto>().ReverseMap();

            CreateMap<CompanyType, CompanyTypeCreateDto>().ReverseMap();
            CreateMap<CompanyType, CompanyTypeUpdateDto>().ReverseMap();
            CreateMap<CompanyType, CompanyTypeResponseDto>().ReverseMap();

            CreateMap<Discount, DiscountCreateDto>().ReverseMap();
            CreateMap<DiscountUpdateDto, Discount>().ForMember(dest => dest.DiscountID, opt => opt.Ignore());
            CreateMap<Discount, DiscountUpdateDto>();
            CreateMap<Discount, DiscountResponseDto>().ReverseMap();

            CreateMap<DiscountType, DiscountTypeCreateDto>().ReverseMap();
            CreateMap<DiscountType, DiscountTypeUpdateDto>().ReverseMap();
            CreateMap<DiscountType, DiscountTypeResponseDto>().ReverseMap();

            CreateMap<DriverInformtion, DriverInformtionCreateDto>().ReverseMap();
            CreateMap<DriverInformtion, DriverInformtionUpdateDto>().ReverseMap();
            CreateMap<DriverInformtion, DriverInformtionResponseDto>().ReverseMap();

            CreateMap<Location, LocationCreateDto>().ReverseMap();
            CreateMap<Location, LocationUpdateDto>().ReverseMap();
            CreateMap<Location, LocationResponseDto>().ReverseMap();

            CreateMap<Payment, PaymentCreateDto>().ReverseMap();
            CreateMap<Payment, PaymentUpdateDto>().ReverseMap();
            CreateMap<Payment, PaymentResponseDto>().ReverseMap();

            CreateMap<PaymentCancel, PaymentCancelCreateDto>().ReverseMap();
            CreateMap<PaymentCancel, PaymentCancelUpdateDto>().ReverseMap();
            CreateMap<PaymentCancel, PaymentCancelResponseDto>().ReverseMap();

            CreateMap<PaymentMethod, PaymentMethodCreateDto>().ReverseMap()
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.LogoUrl));
            CreateMap<PaymentMethod, PaymentMethodUpdateDto>().ReverseMap()
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.LogoUrl));
            CreateMap<PaymentMethod, PaymentMethodResponseDto>().ReverseMap();

            CreateMap<PaymentStatus, PaymentStatusCreateDto>().ReverseMap();
            CreateMap<PaymentStatus, PaymentStatusUpdateDto>().ReverseMap();
            CreateMap<PaymentStatus, PaymentStatusResponseDto>().ReverseMap();

            CreateMap<Refund, RefundCreateDto>().ReverseMap();
            CreateMap<Refund, RefundUpdateDto>().ReverseMap();
            CreateMap<Refund, RefundResponseDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Booking != null ? (src.Booking.User != null ? src.Booking.User.FullName : src.Booking.PassengerName) : null))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Booking != null ? (src.Booking.User != null ? src.Booking.User.Phone : src.Booking.PassengerPhone) : null))
                .ForMember(dest => dest.CustomerImage, opt => opt.MapFrom(src => src.Booking != null && src.Booking.User != null ? src.Booking.User.Picture : null))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Payment != null && src.Payment.PaymentMethod != null ? src.Payment.PaymentMethod.PaymentMethodName : null))
                .ForMember(dest => dest.BookingCode, opt => opt.MapFrom(src => src.Booking != null ? src.Booking.BookingCode : null))
                .ForMember(dest => dest.RefundStatusName, opt => opt.MapFrom(src => src.RefundStatus != null ? src.RefundStatus.StatusName : null))
                .ForMember(dest => dest.UpdatedByUserName, opt => opt.MapFrom(src => src.UpdatedByUser != null ? src.UpdatedByUser.FullName : null));
            CreateMap<RefundResponseDto, Refund>();

            CreateMap<RefundPolicy, RefundPolicyCreateDto>().ReverseMap();
            CreateMap<RefundPolicy, RefundPolicyUpdateDto>().ReverseMap();
            CreateMap<RefundPolicy, RefundPolicyResponseDto>().ReverseMap();

            CreateMap<RefundStatus, RefundStatusCreateDto>().ReverseMap();
            CreateMap<RefundStatus, RefundStatusUpdateDto>().ReverseMap();
            CreateMap<RefundStatus, RefundStatusResponseDto>().ReverseMap();

            CreateMap<Saowari.Models.Entities.Route, RouteCreateDto>().ReverseMap();
            CreateMap<RouteUpdateDto, Saowari.Models.Entities.Route>()
                .ForMember(dest => dest.RouteID, opt => opt.Ignore());
            CreateMap<Saowari.Models.Entities.Route, RouteUpdateDto>();
            CreateMap<Saowari.Models.Entities.Route, RouteResponseDto>().ReverseMap();
            
            CreateMap<Saowari.Models.DTOs.Route.DepartureLocationDto, DepartureLocation>().ReverseMap();

            CreateMap<Schedule, ScheduleCreateDto>().ReverseMap();
            CreateMap<ScheduleUpdateDto, Schedule>()
                .ForMember(dest => dest.ScheduleID, opt => opt.Ignore())
                .ForMember(dest => dest.DepartureLocations, opt => opt.Ignore());
            CreateMap<Schedule, ScheduleUpdateDto>();
            CreateMap<Schedule, ScheduleResponseDto>()
                .ForMember(dest => dest.SeatLayoutConfig, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.SeatLayoutConfig : null))
                .ForMember(dest => dest.DepartureLocations, opt => opt.Ignore())
                .ForMember(dest => dest.SeatClassPricings, opt => opt.MapFrom(src => src.ScheduleSeatClassPricings))
                .ForMember(dest => dest.Route, opt => opt.MapFrom(src => src.Route != null && src.Route.FromLocation != null && src.Route.ToLocation != null ? $"{src.Route.FromLocation.LocationName}-{src.Route.ToLocation.LocationName}" : null))
                .ReverseMap();

            CreateMap<ScheduleSeatStatus, ScheduleSeatStatusCreateDto>().ReverseMap();
            CreateMap<ScheduleSeatStatus, ScheduleSeatStatusUpdateDto>().ReverseMap();
            CreateMap<ScheduleSeatStatus, ScheduleSeatStatusResponseDto>().ReverseMap();

            CreateMap<ScheduleStatus, ScheduleStatusCreateDto>().ReverseMap();
            CreateMap<ScheduleStatus, ScheduleStatusUpdateDto>().ReverseMap();
            CreateMap<ScheduleStatus, ScheduleStatusResponseDto>().ReverseMap();

            CreateMap<Seat, SeatCreateDto>().ReverseMap();
            CreateMap<Seat, SeatUpdateDto>().ReverseMap();
            CreateMap<Seat, SeatResponseDto>().ReverseMap();

            CreateMap<SeatClass, SeatClassCreateDto>().ReverseMap();
            CreateMap<SeatClass, SeatClassUpdateDto>().ReverseMap();
            CreateMap<SeatClass, SeatClassResponseDto>().ReverseMap();

            CreateMap<SeatPricing, SeatPricingCreateDto>().ReverseMap();
            CreateMap<SeatPricing, SeatPricingUpdateDto>().ReverseMap();
            CreateMap<SeatPricing, SeatPricingResponseDto>()
                .ForMember(dest => dest.SeatClassName, opt => opt.MapFrom(src => src.SeatClass != null ? src.SeatClass.SeatClassName : null))
                .ReverseMap();

            CreateMap<ScheduleSeatClassPricing, ScheduleSeatClassPricingDto>()
                .ForMember(dest => dest.SeatClassName, opt => opt.MapFrom(src => src.SeatClass != null ? src.SeatClass.SeatClassName : null))
                .ReverseMap();

            CreateMap<SeatStatus, SeatStatusCreateDto>().ReverseMap();
            CreateMap<SeatStatus, SeatStatusUpdateDto>().ReverseMap();
            CreateMap<SeatStatus, SeatStatusResponseDto>().ReverseMap();

            CreateMap<Supervisor, SupervisorCreateDto>().ReverseMap();
            CreateMap<Supervisor, SupervisorUpdateDto>().ReverseMap();
            CreateMap<Supervisor, SupervisorResponseDto>().ReverseMap();

            CreateMap<Ticket, TicketCreateDto>().ReverseMap();
            CreateMap<Ticket, TicketUpdateDto>().ReverseMap();
            CreateMap<Ticket, TicketResponseDto>().ReverseMap();

            CreateMap<User, UserCreateDto>().ReverseMap();
            CreateMap<User, UserUpdateDto>().ReverseMap();
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.UserRole != null ? src.UserRole.UserRoleName : null))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null));
            CreateMap<UserResponseDto, User>();
            
            CreateMap<UserAdminCreateDto, User>();
            CreateMap<UserAdminUpdateDto, User>();

            CreateMap<UserRole, UserRoleCreateDto>().ReverseMap();
            CreateMap<UserRoleUpdateDto, UserRole>().ForMember(dest => dest.UserRoleId, opt => opt.Ignore());
            CreateMap<UserRole, UserRoleUpdateDto>();
            CreateMap<UserRole, UserRoleResponseDto>().ReverseMap();

            CreateMap<Vehicle, VehicleCreateDto>().ReverseMap();
            CreateMap<VehicleUpdateDto, Vehicle>().ForMember(dest => dest.VehicleID, opt => opt.Ignore());
            CreateMap<Vehicle, VehicleUpdateDto>();
            CreateMap<Vehicle, VehicleResponseDto>().ReverseMap();

            CreateMap<VehicleType, VehicleTypeCreateDto>().ReverseMap();
            CreateMap<VehicleType, VehicleTypeUpdateDto>().ReverseMap();
            CreateMap<VehicleType, VehicleTypeResponseDto>().ReverseMap();

            CreateMap<Notification, Saowari.Models.DTOs.Notification.NotificationDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : null))
                .ReverseMap();

            CreateMap<Saowari.Models.Entities.AdminNotificationPreference, Saowari.Models.DTOs.Notification.AdminNotificationPreferenceDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : "Unknown"))
                .ReverseMap();

            CreateMap<SliderImage, SliderImageCreateDto>().ReverseMap();
            CreateMap<SliderImageUpdateDto, SliderImage>()
                .ForMember(dest => dest.SliderImageID, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());
            CreateMap<SliderImage, SliderImageUpdateDto>();
            CreateMap<SliderImage, SliderImageResponseDto>().ReverseMap();
        }
    }
}
