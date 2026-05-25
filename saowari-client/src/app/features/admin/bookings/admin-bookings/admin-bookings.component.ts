import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BookingService } from '../../../../core/services/api/booking.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-bookings',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-bookings.component.html',
  styleUrls: ['./admin-bookings.component.css']
})
export class AdminBookingsComponent implements OnInit {
  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';
  selectedStatus = '';

  constructor(
    private bookingService: BookingService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void { this.load(); }

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

  cancel(id: number) {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    this.bookingService.cancel(id).subscribe({
      next: (res: any) => {
        if (res.success) { this.notification.success('Booking cancelled.'); this.load(); }
        else this.notification.error(res.message || 'Failed.');
      }
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = { 'Confirmed': 'badge-success', 'Pending': 'badge-warning', 'Cancelled': 'badge-error', 'Completed': 'badge-info' };
    return map[status] || 'badge-ghost';
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  formatTime(d: string): string {
    return new Date(d).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }

  get uniqueStatuses(): string[] {
    return [...new Set(this.items.map(i => i.bookingStatus || i.bookingStatusName).filter(Boolean))] as string[];
  }
}
