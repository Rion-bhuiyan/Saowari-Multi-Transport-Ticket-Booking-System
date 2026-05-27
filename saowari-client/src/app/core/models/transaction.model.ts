export interface BookingModel {
  bookingId: number;
  bookingCode: string;
  userId: number;
  scheduleId: number;
  passengerName: string;
  passengerPhone: string;
  passengerNID?: string;
  baseAmount: number;
  discountAmount: number;
  finalAmount: number;
  discountId?: number;
  bookingStatusId: number;
  bookingStatusName?: string;
  bookingDate: string;
  cancelledAt?: string;
  cancelReason?: string;
  seatClassId?: number;
  seats?: string[];
  schedule?: any;
  hasPendingCancellation?: boolean;
  latestRefundId?: number;
  latestRefundStatusId?: number;
}

export interface BookingCreateDto {
  scheduleId: number;
  passengerName: string;
  passengerPhone: string;
  passengerNID?: string;
  baseAmount: number;
  discountAmount: number;
  finalAmount: number;
  discountId?: number;
  seatClassId?: number;
  seatIds: number[];
}

export interface PaymentModel {
  paymentId: number;
  bookingId: number;
  amount: number;
  discountAmount: number;
  paymentMethodId: number;
  transactionId: string;
  paymentStatusId: number;
  paymentStatusName?: string;
  paidAt?: string;
  createdAt: string;
}

export interface RefundModel {
  refundId: number;
  bookingId: number;
  bookingCode?: string;
  paymentId: number;
  requestedAt: string;
  refundPercentage: number;
  refundAmount: number;
  refundStatusId: number;
  refundStatusName?: string;
  processedAt?: string;
  refundTransactionId?: string;
  remarks?: string;
  isRefunded: boolean;
  policyId?: number;
  requiresOtp?: boolean;  // true when admin approved but user hasn't verified OTP yet
}

export interface RefundPreview {
  bookingId: number;
  paymentId?: number;
  hoursUntilDeparture: number;
  policyName?: string;
  refundPercentage: number;
  eligibleRefundAmount: number;
  originalAmount: number;
  message?: string;
}

export interface TicketModel {
  ticketId: number;
  bookingId: number;
  ticketCode: string;
  issuedAt: string;
  isUsed: boolean;
  usedAt?: string;
}

export interface TicketVerification {
  isValid: boolean;
  ticketCode: string;
  passengerName?: string;
  seatNumber?: string;
  route?: string;
  departureDateTime?: string;
  vehicleName?: string;
  isUsed: boolean;
  status?: string;
}

export interface DiscountModel {
  discountId: number;
  companyId?: number;
  routeId?: number;
  vehicleTypeId?: number;
  discountName: string;
  discountTypeId: number;
  discountValue: number;
  minTicketAmount: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface InvoiceModel {
  invoiceNumber: string;
  bookingCode: string;
  passengerName: string;
  route: string;
  departureDateTime: string;
  seatNumbers: string[];
  seatClass?: string;
  baseAmount: number;
  discountAmount: number;
  finalAmount: number;
  paymentMethod?: string;
  transactionId?: string;
  paidAt?: string;
  ticketCodes: string[];
  issuedAt?: string;
}

export interface DashboardSummary {
  todayBookingsCount: number;
  todayRevenue: number;
  totalActiveRoutes: number;
  totalActiveSchedules: number;
  upcomingDeparturesToday: number;
  bookingsByStatus: any[];
  revenueByPaymentMethod: any[];
}

export interface FareSummary {
  scheduleId: number;
  seatIds: number[];
  baseAmount: number;
  discountAmount: number;
  finalAmount: number;
  discountId?: number;
}
