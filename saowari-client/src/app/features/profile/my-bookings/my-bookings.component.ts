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
  activeTab: 'upcoming' | 'completed' | 'cancelled' = 'upcoming';

  // Refund Modal State
  isRefundModalOpen = false;
  isCalculatingRefund = false;
  isSubmittingRefund = false;
  selectedBookingForRefund: any = null;
  refundPreview: any = null;
  refundRemarks = '';

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
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  get filteredBookings() {
    const now = new Date();
    return this.bookings.filter(b => {
      const dep = new Date(b.departureDateTime);
      if (this.activeTab === 'upcoming') return dep >= now && b.bookingStatus !== 'Cancelled';
      if (this.activeTab === 'completed') return dep < now && b.bookingStatus !== 'Cancelled';
      return b.bookingStatus === 'Cancelled';
    });
  }

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
          this.notification.success('Refund request submitted successfully! Your booking has been cancelled.');
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
