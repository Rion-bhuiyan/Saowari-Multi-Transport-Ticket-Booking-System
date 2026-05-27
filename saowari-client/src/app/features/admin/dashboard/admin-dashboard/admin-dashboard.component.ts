import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../../../core/services/api/dashboard.service';
import { BookingService } from '../../../../core/services/api/booking.service';
import { UserService } from '../../../../core/services/api/user.service';
import { DashboardSummary } from '../../../../core/models/transaction.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, PaginationComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  get pagedItems() {
    const start = (this.p - 1) * Number(this.pageSize);
    return (this.recentBookings || []).slice(start, start + Number(this.pageSize));
  }
  p: number = 1;
  pageSize: number = 15;

  summary: DashboardSummary | null = null;
  recentBookings: any[] = [];
  isLoading = true;

  statsCards = [
    { label: "Today's Bookings", key: 'todayBookingsCount', icon: 'fas fa-clipboard-list', color: 'from-blue-500 to-saowari-primary', suffix: '' },
    { label: "Today's Revenue", key: 'todayRevenue', icon: 'fas fa-bangladeshi-taka-sign', color: 'from-green-400 to-emerald-600', suffix: '৳', prefix: true },
    { label: 'Active Routes', key: 'totalActiveRoutes', icon: 'fas fa-route', color: 'from-purple-500 to-indigo-600', suffix: '' },
    { label: 'Active Schedules', key: 'totalActiveSchedules', icon: 'far fa-calendar-check', color: 'from-orange-400 to-red-500', suffix: '' }
  ];

  quickLinks = [
    { label: 'Manage Users', icon: 'fas fa-users', route: '/admin/users', color: 'text-blue-600 bg-blue-50' },
    { label: 'Companies', icon: 'fas fa-building', route: '/admin/companies', color: 'text-purple-600 bg-purple-50' },
    { label: 'Vehicles', icon: 'fas fa-bus', route: '/admin/vehicles', color: 'text-orange-600 bg-orange-50' },
    { label: 'Routes', icon: 'fas fa-route', route: '/admin/routes', color: 'text-green-600 bg-green-50' },
    { label: 'Schedules', icon: 'far fa-calendar-alt', route: '/admin/schedules', color: 'text-cyan-600 bg-cyan-50' },
    { label: 'Bookings', icon: 'fas fa-ticket-alt', route: '/admin/bookings', color: 'text-red-600 bg-red-50' }
  ];

  constructor(
    private dashboardService: DashboardService,
    private bookingService: BookingService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.loadSummary();
    this.loadRecentBookings();
  }

  loadSummary() {
    this.dashboardService.getSummary().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.summary = res.data;
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  loadRecentBookings() {
    this.bookingService.getAll().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.recentBookings = (res.data || []).slice(0, 8);
        }
      }
    });
  }

  getStat(key: string): any {
    if (!this.summary) return 0;
    return (this.summary as any)[key] ?? 0;
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
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
