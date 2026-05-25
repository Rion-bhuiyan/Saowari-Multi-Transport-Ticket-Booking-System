export interface LocationModel {
  locationId: number;
  locationName: string;
  locationCode?: number;
  latitude?: number;
  longitude?: string;
  district?: string;
  isActive: boolean;
}

export interface RouteModel {
  routeId: number;
  fromLocationId: number;
  toLocationId: number;
  fromLocation?: string;
  toLocation?: string;
  distanceKM?: number;
  estimatedHours?: number;
  isActive: boolean;
}

export interface VehicleModel {
  vehicleId: number;
  companyId: number;
  vehicleName: string;
  vehicleNumber: string;
  vehicleType?: string;
  totalSeats: number;
  isActive: boolean;
  seatClassPricings?: any[];
}

export interface SeatModel {
  seatId: number;
  vehicleId: number;
  seatNumber: string;
  seatPriceing?: number;
  seatClassId?: number;
  seatClassName?: string;
  isActive: boolean;
}

export interface ScheduleModel {
  scheduleId: number;
  routeId: number;
  vehicleId: number;
  driverInformtionId?: number;
  supervisorId?: number;
  departureDateTime: string;
  arrivalDateTime: string;
  basePrice: number;
  availableSeats: number;
  scheduleStatusId: number;
  scheduleStatusName?: string;
  route?: any;
  vehicle?: string;
  seatLayoutConfig?: string;
  departureLocations?: any[];
  seatClassPricings?: any[];
}

export interface SliderImageModel {
  sliderImageID: number;
  imageUrl: string;
  title?: string;
  subtitle?: string;
  linkUrl?: string;
  displayOrder: number;
  isActive: boolean;
}
