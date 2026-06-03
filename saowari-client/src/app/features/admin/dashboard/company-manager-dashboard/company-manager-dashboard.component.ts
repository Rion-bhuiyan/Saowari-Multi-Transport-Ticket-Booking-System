import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { VehicleService } from '../../../../core/services/api/vehicle.service';
import { BookingService } from '../../../../core/services/api/booking.service';

@Component({
  selector: 'app-company-manager-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, PaginationComponent],
  templateUrl: './company-manager-dashboard.component.html',
  styleUrls: ['./company-manager-dashboard.component.css']
})
export class CompanyManagerDashboardComponent implements OnInit {
  get pagedItems() {
    const start = (this.p - 1) * Number(this.pageSize);
    return (this.schedules || []).slice(start, start + Number(this.pageSize));
  }
  p: number = 1;
  pageSize: number = 5;

  isLoading = true;
  totalSchedules = 0;
  totalVehicles = 0;
  activeSchedules = 0;
  completedSchedules = 0;
  totalRevenue = 0;
  totalBookings = 0;
  totalPassengers = 0;

  schedules: any[] = [];
  leaderboard: any[] = [];

  quickLinks = [
    { label: 'Add Vehicle', icon: 'fas fa-bus', route: '/admin/vehicles', gradient: 'from-orange-400 to-orange-600', shadow: 'shadow-orange-500/30' },
    { label: 'New Schedule', icon: 'far fa-calendar-plus', route: '/admin/schedules', gradient: 'from-green-400 to-green-600', shadow: 'shadow-green-500/30' },
    { label: 'Refunds', icon: 'fas fa-undo-alt', route: '/admin/refunds', gradient: 'from-blue-400 to-blue-600', shadow: 'shadow-blue-500/30' },
    { label: 'Discounts', icon: 'fas fa-tags', route: '/admin/discounts', gradient: 'from-purple-400 to-purple-600', shadow: 'shadow-purple-500/30' },
    { label: 'Reports', icon: 'fas fa-chart-line', route: '/admin/reports', gradient: 'from-pink-400 to-pink-600', shadow: 'shadow-pink-500/30' },
    { label: 'Add Route', icon: 'fas fa-route', route: '/admin/routes', gradient: 'from-teal-400 to-teal-600', shadow: 'shadow-teal-500/30' }
  ];

  today = new Date();
  private reqCount = 0;
  private readonly totalReqs = 3;

  constructor(
    private scheduleService: ScheduleService,
    private vehicleService: VehicleService,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.reqCount = 0;

    // 1. Fetch Schedules
    this.scheduleService.getAll().subscribe((res: any) => {
      if (res.success && res.data) {
        this.schedules = res.data;
        this.totalSchedules = this.schedules.length;
        this.activeSchedules = this.schedules.filter((s: any) => {
          const statusName = s.scheduleStatusName?.toLowerCase() || s.statusName?.toLowerCase() || '';
          return statusName.includes('active') || statusName.includes('scheduled');
        }).length;
        this.completedSchedules = this.schedules.filter((s: any) => {
          const statusName = s.scheduleStatusName?.toLowerCase() || s.statusName?.toLowerCase() || '';
          return statusName.includes('complet') || statusName.includes('expir');
        }).length;
      }
      this.checkLoading();
    });

    // 2. Fetch Vehicles
    this.vehicleService.getAll().subscribe((res: any) => {
      if (res.success && res.data) {
        this.totalVehicles = res.data.length;
      }
      this.checkLoading();
    });

    // 3. Fetch Bookings (Already filtered by CompanyId in backend)
    this.bookingService.getAll().subscribe((res: any) => {
      if (res.success && res.data) {
        const bookings = res.data;
        this.totalBookings = bookings.length;
        this.totalRevenue = bookings.reduce((sum: number, b: any) => sum + (b.finalAmount || 0), 0);
        this.totalPassengers = bookings.reduce((sum: number, b: any) => sum + (b.bookingSeats?.length || 1), 0);
        
        // Calculate Leaderboard (Top 5 passengers by revenue)
        const revenueMap = new Map<string, { phone: string, name: string, totalAmount: number, tickets: number }>();
        
        for (const b of bookings) {
          if (!b.passengerPhone) continue;
          const phone = b.passengerPhone;
          const entry = revenueMap.get(phone) || { phone, name: b.passengerName || 'Unknown', totalAmount: 0, tickets: 0 };
          entry.totalAmount += (b.finalAmount || 0);
          entry.tickets += 1;
          revenueMap.set(phone, entry);
        }

        this.leaderboard = Array.from(revenueMap.values())
          .sort((a, b) => b.totalAmount - a.totalAmount)
          .slice(0, 5);
      }
      this.checkLoading();
    });
  }

  checkLoading() {
    this.reqCount++;
    if (this.reqCount >= this.totalReqs) {
      this.isLoading = false;
    }
  }

  getStatusBadge(name: string): string {
    const s = (name || '').toLowerCase();
    if (s.includes('scheduled') || s.includes('active')) return 'bg-green-100 text-green-700 border-green-200';
    if (s.includes('cancel')) return 'bg-red-100 text-red-700 border-red-200';
    if (s.includes('complet') || s.includes('expir')) return 'bg-blue-100 text-blue-700 border-blue-200';
    if (s.includes('pending')) return 'bg-yellow-100 text-yellow-700 border-yellow-200';
    return 'bg-gray-100 text-gray-700 border-gray-200';
  }
}
