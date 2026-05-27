import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { VehicleService } from '../../../../core/services/api/vehicle.service';

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
  pageSize: number = 10;

  isLoading = true;
  totalSchedules = 0;
  totalVehicles = 0;
  activeSchedules = 0;
  completedSchedules = 0;
  totalRevenue = 0;

  schedules: any[] = [];

  quickLinks = [
    { label: 'Add Vehicle', icon: 'fas fa-bus', route: '/admin/vehicles', color: 'text-orange-600 bg-orange-50' },
    { label: 'New Schedule', icon: 'far fa-calendar-plus', route: '/admin/schedules', color: 'text-green-600 bg-green-50' },
    { label: 'Manage Refunds', icon: 'fas fa-undo-alt', route: '/admin/refunds', color: 'text-blue-600 bg-blue-50' },
    { label: 'View Reports', icon: 'fas fa-chart-line', route: '/admin/reports', color: 'text-purple-600 bg-purple-50' }
  ];

  today = new Date();

  constructor(
    private scheduleService: ScheduleService,
    private vehicleService: VehicleService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.reqCount = 0;

    // Fetch all schedules for this company
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

    // Fetch Vehicles
    this.vehicleService.getAll().subscribe((res: any) => {
      if (res.success && res.data) {
        this.totalVehicles = res.data.length;
      }
      this.checkLoading();
    });
  }

  private reqCount = 0;
  checkLoading() {
    this.reqCount++;
    if (this.reqCount >= 2) {
      this.isLoading = false;
    }
  }

  getStatusBadge(name: string): string {
    const s = (name || '').toLowerCase();
    if (s.includes('scheduled') || s.includes('active')) return 'badge-success';
    if (s.includes('cancel')) return 'badge-error';
    if (s.includes('complet') || s.includes('expir')) return 'badge-info';
    if (s.includes('pending')) return 'badge-warning';
    return 'badge-ghost';
  }
}
