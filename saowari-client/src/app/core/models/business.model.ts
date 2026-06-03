export interface TripSearchResult {
  scheduleId: number;
  vehicleId: number;
  vehicleName: string;
  vehicleNumber: string;
  vehicleType: string;
  fromLocation: string;
  toLocation: string;
  departureDateTime: string;
  arrivalDateTime: string;
  basePrice: number;
  availableSeats: number;
  companyName?: string;
  companyLogo?: string;
  seatLayoutConfig?: string;
  seatClassOptions?: any;
  boardingTime?: string;
  showDetails?: boolean;
}

export interface SeatMapItem {
  statusId: number;
  scheduleId: number;
  seatId: number;
  bookingId?: number;
  seatStatusId: number;
  seatStatusName: string;
  seatNumber: string;
  SeatNumber?: string; // Fallback for PascalCase response
  seatClassId?: number;
  seatClassName?: string;
  SeatClassName?: string; // Fallback for PascalCase response
  price?: number;
  Price?: number; // Fallback for PascalCase response
  isBooked: boolean;
}

export interface FareSummary {
  baseAmount: number;
  discountAmount: number;
  finalAmount: number;
  discountName?: string;
}
