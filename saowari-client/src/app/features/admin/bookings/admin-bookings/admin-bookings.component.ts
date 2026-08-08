import { Component, OnInit, OnDestroy } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookingService } from '../../../../core/services/api/booking.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { NotificationService as RealTimeNotifService } from '../../../../core/services/api/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-admin-bookings',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-bookings.component.html',
  styleUrls: ['./admin-bookings.component.css']
})
export class AdminBookingsComponent implements OnInit, OnDestroy {
  get pagedItems() {
    const start = (this.p - 1) * Number(this.pageSize);
    return (this.filtered || this.items || []).slice(start, start + Number(this.pageSize));
  }
  p: number = 1;
  pageSize: number = 15;

  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';
  selectedStatus = '';
  private adminDataSub?: Subscription;

  // OTP Modal State
  showOtpModal = false;
  otpInput = '';
  pendingCancelId: number | null = null;
  isVerifying = false;
  isRequesting = false;

  constructor(
    private bookingService: BookingService,
    private notification: NotificationService,
    private realTimeNotif: RealTimeNotifService
  ) {}

  ngOnInit(): void { 
    this.load(); 
    this.adminDataSub = this.realTimeNotif.adminDataUpdated$.subscribe(dataType => {
      if (dataType === 'Booking') {
        this.loadSilent();
      }
    });
  }

  ngOnDestroy(): void {
    if (this.adminDataSub) {
      this.adminDataSub.unsubscribe();
    }
  }

  loadSilent() {
    this.bookingService.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { this.items = res.data || []; this.applyFilter(); }
      }
    });
  }

  load() {
    this.isLoading = true;
    this.bookingService.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { this.items = res.data || []; this.applyFilter(); }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  applyFilter() {
    let data = [...this.items];
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      data = data.filter(i =>
        i.bookingCode?.toLowerCase().includes(q) ||
        i.passengerName?.toLowerCase().includes(q) ||
        i.confirmationCode?.toLowerCase().includes(q)
      );
    }
    if (this.selectedStatus) data = data.filter(i => (i.bookingStatus || i.bookingStatusName) === this.selectedStatus);
    this.filtered = data;
  }

  initiateCancel(id: number) {
    if (!confirm('Are you sure you want to cancel this booking? This will send an OTP to the user.')) return;
    
    this.isRequesting = true;
    this.bookingService.requestCancel(id).subscribe({
      next: (res: any) => {
        this.isRequesting = false;
        if (res.success) {
          this.pendingCancelId = id;
          this.otpInput = '';
          this.showOtpModal = true;
          this.notification.success('An OTP has been sent to the user email and notification panel.');
        } else {
          this.notification.error(res.message || 'Failed to request cancellation.');
        }
      },
      error: () => {
        this.isRequesting = false;
        this.notification.error('Failed to request cancellation.');
      }
    });
  }

  verifyCancel() {
    if (!this.pendingCancelId) return;
    if (!this.otpInput || this.otpInput.length < 5) {
      this.notification.error('Please enter a valid OTP.');
      return;
    }

    this.isVerifying = true;
    this.bookingService.verifyCancel(this.pendingCancelId, this.otpInput).subscribe({
      next: (res: any) => {
        this.isVerifying = false;
        if (res.success) {
          this.notification.success('Booking successfully cancelled!');
          this.closeOtpModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Invalid OTP. Please try again.');
        }
      },
      error: () => {
        this.isVerifying = false;
        this.notification.error('Invalid OTP. Please try again.');
      }
    });
  }

  closeOtpModal() {
    this.showOtpModal = false;
    this.pendingCancelId = null;
    this.otpInput = '';
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = { 
      'Confirmed': 'bg-green-100 text-green-700 border-green-200', 
      'Pending': 'bg-amber-100 text-amber-700 border-amber-200', 
      'Cancelled': 'bg-red-100 text-red-700 border-red-200', 
      'Completed': 'bg-blue-100 text-blue-700 border-blue-200' 
    };
    return map[status] || 'bg-gray-100 text-gray-700 border-saowari-border';
  }

  formatDate(d: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  formatTime(d: string): string {
    if (!d) return '';
    return new Date(d).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }

  get uniqueStatuses(): string[] {
    return [...new Set(this.items.map(i => i.bookingStatus || i.bookingStatusName).filter(Boolean))] as string[];
  }
}
