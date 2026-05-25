import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { BookingService } from '../../core/services/api/booking.service';
import { ScheduleService } from '../../core/services/api/schedule.service';
import { PaymentService } from '../../core/services/api/payment.service';
import { NotificationService } from '../../core/services/notification.service';
import { AuthService } from '../../core/services/auth.service';
import { ScheduleModel } from '../../core/models/master.model';
import { BookingStateService } from '../../core/services/booking-state.service';

import { PaymentMethodService, PaymentMethodModel } from '../../core/services/api/payment-method.service';
import { SearchService } from '../../core/services/api/search.service';

interface CustomerInfo {
  passengerName: string;
  age: number | null;
  gender: string;
  mobileNumber: string;
}

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css']
})
export class BookingComponent implements OnInit {
  currentStep = 1;
  totalSteps = 3;

  scheduleId!: number;
  schedule!: ScheduleModel;
  customer: CustomerInfo = {
    passengerName: '',
    age: null,
    gender: 'Male',
    mobileNumber: ''
  };
  seatNumbers: string[] = [];
  seatIds: number[] = [];
  seatPrices: { [seatId: number]: number } = {};
  returnSeatPrices: { [seatId: number]: number } = {};

  selectedBoardingPoint: string = '';
  boardingOptions: any[] = [];

  isRoundTrip = false;
  returnScheduleId?: number;
  returnSchedule?: ScheduleModel;
  returnSeatIds: number[] = [];
  returnSeatNumbers: string[] = [];
  
  selectedReturnBoardingPoint: string = '';
  returnBoardingOptions: any[] = [];

  paymentMethods: PaymentMethodModel[] = [];
  selectedMethod: PaymentMethodModel | null = null;
  paymentMethod = ''; // string binding to match selectedMethod.paymentMethodName
  mobileForPayment = '';
  transactionId = '';

  createdBookingId: number | null = null;
  bookingConfirmationCode = '';
  isProcessing = false;

  couponCodeInput = '';
  isCouponApplied = false;
  isApplyingCoupon = false;
  couponMessage = '';
  discountAmount = 0;
  discountId: number | null = null;
  isPercentageDiscount: boolean = false;
  discountValue: number = 0;

  currentUser$ = this.authService.currentUser$;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bookingService: BookingService,
    private scheduleService: ScheduleService,
    private searchService: SearchService,
    private paymentService: PaymentService,
    private pmService: PaymentMethodService,
    private notification: NotificationService,
    private authService: AuthService,
    public bookingState: BookingStateService
  ) {}

  ngOnInit(): void {
    this.loadPaymentMethods();
    const state = this.bookingState.currentState;
    
    if (state.tripType === 'round-way' && state.outbound.schedule && state.return.schedule) {
       this.isRoundTrip = true;
       this.scheduleId = state.outbound.schedule.scheduleId;
       this.seatIds = state.outbound.seatIds;
       this.seatNumbers = state.outbound.seatNumbers;
       
       this.returnScheduleId = state.return.schedule.scheduleId;
       this.returnSeatIds = state.return.seatIds;
       this.returnSeatNumbers = state.return.seatNumbers;

       this.loadSchedule(this.scheduleId, false);
       this.loadSchedule(this.returnScheduleId, true);
       this.initPassengers();
    } else if (state.outbound.schedule) {
       this.scheduleId = state.outbound.schedule.scheduleId;
       this.seatIds = state.outbound.seatIds;
       this.seatNumbers = state.outbound.seatNumbers;
       this.loadSchedule(this.scheduleId, false);
       this.initPassengers();
    } else {
      this.route.queryParams.subscribe(params => {
        if (params['scheduleId']) this.scheduleId = +params['scheduleId'];
        if (params['seats']) this.seatIds = params['seats'].split(',').map((s: string) => +s);
        if (params['seatNumbers']) {
          this.seatNumbers = params['seatNumbers'].split(',');
        } else {
          this.seatNumbers = this.seatIds.map((_, i) => `Seat ${i + 1}`);
        }
        this.loadSchedule(this.scheduleId, false);
        this.initPassengers();
      });
    }
  }

  loadSchedule(id: number, isReturn: boolean) {
    if (!id) return;
    this.scheduleService.getById(id).subscribe(res => {
      if (res.success && res.data) {
        if (isReturn) {
          this.returnSchedule = res.data;
          if (this.returnSchedule.departureLocations && this.returnSchedule.departureLocations.length > 0) {
            const unique = new Set<string>();
            this.returnSchedule.departureLocations.forEach((dl: any) => {
              const timeStr = dl.time ? dl.time.substring(0, 5) : '00:00';
              unique.add(`${dl.locationName} (${timeStr})`);
            });
            this.returnBoardingOptions = Array.from(unique);
            if (this.returnBoardingOptions.length > 0) {
              const stateBoardingPoint = this.bookingState.currentState.return.selectedBoardingPoint;
              const matchedState = this.returnBoardingOptions.find(opt => 
                stateBoardingPoint && opt.toLowerCase().includes(stateBoardingPoint.toLowerCase())
              );
              if (matchedState) {
                this.selectedReturnBoardingPoint = matchedState;
              } else {
                const searchedFrom = this.bookingState.currentState.return.schedule?.fromLocation || '';
                const matchedOpt = this.returnBoardingOptions.find(opt => 
                  searchedFrom && opt.toLowerCase().includes(searchedFrom.toLowerCase())
                );
                this.selectedReturnBoardingPoint = matchedOpt || this.returnBoardingOptions[0];
              }
            }
          }
        } else {
          this.schedule = res.data;
          if (this.schedule.departureLocations && this.schedule.departureLocations.length > 0) {
            const unique = new Set<string>();
            this.schedule.departureLocations.forEach((dl: any) => {
              const timeStr = dl.time ? dl.time.substring(0, 5) : '00:00';
              unique.add(`${dl.locationName} (${timeStr})`);
            });
            this.boardingOptions = Array.from(unique);
            if (this.boardingOptions.length > 0) {
              const stateBoardingPoint = this.bookingState.currentState.outbound.selectedBoardingPoint;
              const matchedState = this.boardingOptions.find(opt => 
                stateBoardingPoint && opt.toLowerCase().includes(stateBoardingPoint.toLowerCase())
              );
              if (matchedState) {
                this.selectedBoardingPoint = matchedState;
              } else {
                const searchedFrom = this.bookingState.currentState.outbound.schedule?.fromLocation || '';
                const matchedOpt = this.boardingOptions.find(opt => 
                  searchedFrom && opt.toLowerCase().includes(searchedFrom.toLowerCase())
                );
                this.selectedBoardingPoint = matchedOpt || this.boardingOptions[0];
              }
            }
          }
        }
      }
    });

    this.searchService.getSeatMap(id).subscribe(res => {
      if (res.success && res.data) {
        res.data.forEach((seat: any) => {
          const price = seat.price ?? seat.Price ?? (isReturn ? this.returnSchedule?.basePrice : this.schedule?.basePrice) ?? 0;
          if (isReturn) {
            this.returnSeatPrices[seat.seatId ?? seat.SeatId] = price;
          } else {
            this.seatPrices[seat.seatId ?? seat.SeatId] = price;
          }
        });
      }
    });
  }

  initPassengers() {
    // No longer needed — single customer form
  }

  getTotalFare(): number {
    let total = 0;
    this.seatIds.forEach(id => {
      total += this.seatPrices[id] ?? this.schedule?.basePrice ?? 0;
    });
    if (this.isRoundTrip && this.returnSchedule) {
      this.returnSeatIds.forEach(id => {
        total += this.returnSeatPrices[id] ?? this.returnSchedule?.basePrice ?? 0;
      });
    }
    return total;
  }

  getFinalFare(): number {
    const total = this.getTotalFare() - this.discountAmount;
    const fee = this.getProcessingFee();
    const vat = this.getVAT();
    return Math.max(0, total + fee + vat);
  }

  loadPaymentMethods() {
    this.pmService.getActive().subscribe({
      next: res => {
        this.paymentMethods = res.data || [];
        if (this.paymentMethods.length > 0) {
          // Select bKash or first active as default
          const defaultPm = this.paymentMethods.find(m => m.paymentMethodName.toLowerCase() === 'bkash') || this.paymentMethods[0];
          this.selectMethod(defaultPm);
        }
      }
    });
  }

  selectMethod(pm: PaymentMethodModel) {
    this.selectedMethod = pm;
    this.paymentMethod = pm.paymentMethodName;
  }

  getProcessingFee(): number {
    if (this.currentStep < 2) return 0;
    if (!this.selectedMethod || this.selectedMethod.processingFeePercent <= 0) return 0;
    const base = this.getTotalFare() - this.discountAmount;
    return parseFloat((base * this.selectedMethod.processingFeePercent / 100).toFixed(2));
  }

  getVAT(): number {
    if (this.currentStep < 2) return 0;
    if (!this.selectedMethod || this.selectedMethod.vatPercent <= 0) return 0;
    const base = this.getTotalFare();
    return parseFloat((base * this.selectedMethod.vatPercent / 100).toFixed(2));
  }

  applyCoupon() {
    if (!this.couponCodeInput) return;
    this.isApplyingCoupon = true;
    this.couponMessage = '';

    const code = this.couponCodeInput.trim().toUpperCase();

    if (code === 'SAOWARI6') {
      setTimeout(() => {
        this.isApplyingCoupon = false;
        this.isCouponApplied = true;
        this.discountAmount = Math.round(this.getTotalFare() * 0.10);
        this.discountId = null;
        this.couponMessage = '10% private discount applied successfully!';
      }, 300);
      return;
    }

    const payload = {
      scheduleId: this.scheduleId,
      couponCode: this.couponCodeInput.trim(),
      totalTicketAmount: this.getTotalFare()
    };

    // We will call the API using standard http client
    this.bookingService.validateCoupon(payload).subscribe({
      next: (res: any) => {
        this.isApplyingCoupon = false;
        if (res.data?.isValid) {
          this.isCouponApplied = true;
          this.discountAmount = res.data.discountAmount;
          this.discountId = res.data.discountId;
          this.isPercentageDiscount = res.data.isPercentage;
          this.discountValue = res.data.discountValue;
          this.couponMessage = res.data.message || 'Coupon applied successfully!';
        } else {
          this.isCouponApplied = false;
          this.discountAmount = 0;
          this.discountId = null;
          this.couponMessage = res.data?.message || res.message || 'Invalid coupon code.';
        }
      },
      error: (err) => {
        this.isApplyingCoupon = false;
        this.isCouponApplied = false;
        this.couponMessage = err?.error?.message || 'Failed to validate coupon.';
      }
    });
  }

  removeCoupon() {
    this.couponCodeInput = '';
    this.isCouponApplied = false;
    this.discountAmount = 0;
    this.discountId = null;
    this.couponMessage = '';
  }

  nextStep(form?: NgForm) {
    if (form && form.invalid) {
      form.control.markAllAsTouched();
      this.notification.warning('Please fill in all required fields.');
      return;
    }
    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  confirmBooking() {
    this.isProcessing = true;

    const passengerPayload = this.seatIds.map(seatId => ({
      seatId,
      passengerName: this.customer.passengerName,
      age: this.customer.age,
      gender: this.customer.gender,
      mobileNumber: this.customer.mobileNumber
    }));

    const outboundPayload = {
      scheduleId: this.scheduleId,
      seatIds: this.seatIds,
      passengers: passengerPayload,
      paymentMethod: this.paymentMethod,
      mobileNumber: this.mobileForPayment,
      transactionId: this.transactionId,
      discountId: this.discountId,
      boardingPoint: this.selectedBoardingPoint
    };

    this.bookingService.create(outboundPayload as any).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.createdBookingId = res.data?.bookingID || res.data?.bookingId || res.data?.id;
          this.bookingConfirmationCode = res.data?.bookingCode || res.data?.confirmationCode || `SAO-${Date.now()}`;
          
          if (this.isRoundTrip && this.returnScheduleId) {
             const returnPayload = {
               scheduleId: this.returnScheduleId,
               seatIds: this.returnSeatIds,
              passengers: this.returnSeatIds.map(rSeatId => ({
                  seatId: rSeatId,
                  passengerName: this.customer.passengerName,
                  age: this.customer.age,
                  gender: this.customer.gender,
                  mobileNumber: this.customer.mobileNumber
                })),
               paymentMethod: this.paymentMethod,
               mobileNumber: this.mobileForPayment,
               transactionId: this.transactionId ? this.transactionId + '-RET' : '',
               boardingPoint: this.selectedReturnBoardingPoint
             };

             this.bookingService.create(returnPayload as any).subscribe({
                next: (retRes: any) => {
                   if (retRes.success) {
                      this.bookingConfirmationCode += ` & ${retRes.data?.confirmationCode || 'SAO-RET'}`;
                   }
                   this.finalizeCheckout();
                },
                error: () => this.finalizeCheckout()
             });
          } else {
             this.finalizeCheckout();
          }

        } else {
          this.notification.error(res.message || 'Booking failed. Please try again.', 'Error');
          this.isProcessing = false;
        }
      },
      error: () => {
        this.notification.error('An error occurred while processing your booking.', 'Error');
        this.isProcessing = false;
      }
    });
  }

  finalizeCheckout() {
    this.currentStep = 3;
    this.notification.success('Booking confirmed successfully!', 'Success');
    this.bookingState.clearState();
    window.scrollTo({ top: 0, behavior: 'smooth' });
    this.isProcessing = false;
  }

  downloadTicket() {
    if (this.bookingConfirmationCode) {
      const nameSlug = (this.customer?.passengerName || 'Passenger')
        .replace(/\s+/g, '')
        .replace(/[^a-zA-Z0-9]/g, '');
      const slug = `${this.bookingConfirmationCode}-${nameSlug}`;
      window.open(`/ticket/${slug}`, '_blank');
    }
  }

  goHome() {
    this.router.navigate(['/home']);
  }

  formatTime(dateString: string | undefined): string {
    if (!dateString) return '';
    return new Date(dateString).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return '';
    return new Date(dateString).toLocaleDateString('en-US', { weekday: 'short', year: 'numeric', month: 'long', day: 'numeric' });
  }
}
