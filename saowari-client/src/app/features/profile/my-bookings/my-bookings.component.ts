import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookingService } from '../../../core/services/api/booking.service';
import { RefundService } from '../../../core/services/api/refund.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './my-bookings.component.html',
  styleUrls: ['./my-bookings.component.css']
})
export class MyBookingsComponent implements OnInit {
  bookings: any[] = [];
  isLoading = true;
  activeTab: 'upcoming' | 'pending' | 'completed' | 'cancelled' = 'upcoming';

  // Refund Modal State
  isRefundModalOpen = false;
  isCalculatingRefund = false;
  isSubmittingRefund = false;
  selectedBookingForRefund: any = null;
  refundPreview: any = null;
  refundRemarks = '';

  // Cancel/Refund OTP Modal State
  isCancelOtpModalOpen = false;
  selectedBookingForCancel: any = null;
  cancelOtp = '';
  isRequestingOtp = false;
  isVerifyingOtp = false;
  otpRequested = false;
  otpMode: 'cancel' | 'refund' = 'cancel';

  constructor(
    private bookingService: BookingService,
    private refundService: RefundService,
    private notification: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings() {
    this.isLoading = true;
    this.bookingService.getMy().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.bookings = res.data || [];
          // Auto-switch to pending tab if any bookings have pending cancellations
          if (this.activeTab === 'upcoming' && this.pendingCancellationCount > 0) {
            // Don't auto-switch, just show badge
          }
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  get filteredBookings() {
    const now = new Date();
    return this.bookings.filter(b => {
      const isPendingRefund = b.latestRefundStatusId === 1 || b.latestRefundStatusId === 2;
      const isPendingCancel = b.hasPendingCancellation === true;
      const isPending = isPendingRefund || isPendingCancel;

      if (this.activeTab === 'pending') return isPending;
      
      if (this.activeTab === 'upcoming') {
        const dep = new Date(b.departureDateTime);
        return dep >= now && b.bookingStatus !== 'Cancelled' && !isPending;
      }
      
      if (this.activeTab === 'completed') {
        const dep = new Date(b.departureDateTime);
        return dep < now && b.bookingStatus !== 'Cancelled' && !isPending;
      }
      
      return b.bookingStatus === 'Cancelled';
    });
  }

  get pendingCancellationCount(): number {
    return this.bookings.filter(b => b.hasPendingCancellation === true || b.latestRefundStatusId === 1 || b.latestRefundStatusId === 2).length;
  }

  // ===== Cancellation/Refund OTP Flow =====
  openCancelModal(booking: any) {
    this.selectedBookingForCancel = booking;
    this.cancelOtp = '';
    this.otpMode = 'cancel';
    this.otpRequested = booking.hasPendingCancellation === true;
    this.isCancelOtpModalOpen = true;
  }

  openRefundOtpModal(booking: any) {
    this.selectedBookingForCancel = booking;
    this.cancelOtp = '';
    this.otpMode = 'refund';
    this.otpRequested = true; // OTP already sent by admin
    this.isCancelOtpModalOpen = true;
  }

  closeCancelModal() {
    this.isCancelOtpModalOpen = false;
    this.selectedBookingForCancel = null;
    this.cancelOtp = '';
    this.otpRequested = false;
    this.otpMode = 'cancel';
  }

  requestCancellationOtp() {
    if (!this.selectedBookingForCancel) return;
    const id = this.selectedBookingForCancel.bookingID || this.selectedBookingForCancel.bookingId;
    this.isRequestingOtp = true;
    this.bookingService.requestCancel(id).subscribe({
      next: (res: any) => {
        this.isRequestingOtp = false;
        if (res.success) {
          this.otpRequested = true;
          this.notification.success('OTP sent to your registered email. Please check your inbox.', 'OTP Sent');
          this.loadBookings(); // Refresh to show pending tab
        } else {
          this.notification.error(res.message || 'Failed to request cancellation OTP.', 'Error');
        }
      },
      error: (err: any) => {
        this.isRequestingOtp = false;
        this.notification.error(err?.error?.message || 'Error sending OTP.', 'Error');
      }
    });
  }

  verifyOtp() {
    if (!this.cancelOtp || this.cancelOtp.length !== 6) {
      this.notification.warning('Please enter the 6-digit OTP from your email.');
      return;
    }
    const id = this.selectedBookingForCancel.bookingID || this.selectedBookingForCancel.bookingId;
    this.isVerifyingOtp = true;

    if (this.otpMode === 'refund') {
      const refundId = this.selectedBookingForCancel.latestRefundId;
      this.refundService.verifyRefundOtp(refundId, this.cancelOtp).subscribe({
        next: (res: any) => {
          this.isVerifyingOtp = false;
          if (res.success) {
            this.notification.success('Refund completed successfully!', 'Success');
            this.closeCancelModal();
            this.activeTab = 'cancelled';
            this.loadBookings();
          } else {
            this.notification.error(res.message || 'Invalid or expired OTP. Please try again.', 'Verification Failed');
          }
        },
        error: (err: any) => {
          this.isVerifyingOtp = false;
          this.notification.error(err?.error?.message || 'Invalid OTP.', 'Error');
        }
      });
    } else {
      this.bookingService.verifyCancel(id, this.cancelOtp).subscribe({
        next: (res: any) => {
          this.isVerifyingOtp = false;
          if (res.success) {
            this.notification.success('Booking cancelled successfully! Your seat has been released.', 'Booking Cancelled');
            this.closeCancelModal();
            this.activeTab = 'cancelled';
            this.loadBookings();
          } else {
            this.notification.error(res.message || 'Invalid or expired OTP. Please try again.', 'Verification Failed');
          }
        },
        error: (err: any) => {
          this.isVerifyingOtp = false;
          this.notification.error(err?.error?.message || 'Invalid OTP.', 'Error');
        }
      });
    }
  }

  // ===== Refund Flow =====
  requestRefund(booking: any) {
    this.openRefundModal(booking);
  }

  openRefundModal(booking: any) {
    this.selectedBookingForRefund = booking;
    this.isRefundModalOpen = true;
    this.isCalculatingRefund = true;
    this.refundPreview = null;
    this.refundRemarks = '';

    const bookingId = booking.bookingID || booking.bookingId || booking.id;
    this.refundService.calculate(bookingId).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.refundPreview = res.data;
        } else {
          this.notification.error(res.message || 'Failed to calculate eligible refund amount.');
          this.closeRefundModal();
        }
        this.isCalculatingRefund = false;
      },
      error: (err: any) => {
        this.notification.error(err.error?.message || 'Error calculating eligible refund.');
        this.closeRefundModal();
        this.isCalculatingRefund = false;
      }
    });
  }

  closeRefundModal() {
    this.isRefundModalOpen = false;
    this.selectedBookingForRefund = null;
    this.refundPreview = null;
    this.refundRemarks = '';
  }

  submitRefundRequest() {
    if (!this.selectedBookingForRefund) return;
    const bookingId = this.selectedBookingForRefund.bookingID || this.selectedBookingForRefund.bookingId || this.selectedBookingForRefund.id;
    this.isSubmittingRefund = true;
    this.refundService.requestCustomerRefund(bookingId, this.refundRemarks).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Refund request submitted! Your booking has been cancelled.');
          this.closeRefundModal();
          this.loadBookings();
        } else {
          this.notification.error(res.message || 'Failed to submit refund request.');
          this.isSubmittingRefund = false;
        }
      },
      error: (err: any) => {
        this.notification.error(err.error?.message || 'Error submitting refund request.');
        this.isSubmittingRefund = false;
      }
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Confirmed': 'badge-success',
      'Pending': 'badge-warning',
      'Cancelled': 'badge-error',
      'Completed': 'badge-info'
    };
    return map[status] || 'badge-ghost';
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }

  formatTime(d: string): string {
    return new Date(d).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
}
