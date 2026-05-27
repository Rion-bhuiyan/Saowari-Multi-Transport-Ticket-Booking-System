import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../core/services/api/schedule.service';

@Component({
  selector: 'app-supervisor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './supervisor-dashboard.component.html',
  styleUrls: ['./supervisor-dashboard.component.css']
})
export class SupervisorDashboardComponent implements OnInit {
  isLoading = true;
  schedules: any[] = [];
  upcomingSchedules: any[] = [];
  ongoingSchedules: any[] = [];
  completedSchedules: any[] = [];

  today = new Date();

  get totalTrips() { return this.schedules.length; }
  get ongoingCount() { return this.ongoingSchedules.length; }
  get upcomingCount() { return this.upcomingSchedules.length; }
  get completedCount() { return this.completedSchedules.length; }
  
  constructor(private scheduleService: ScheduleService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.scheduleService.getLifecycle().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.upcomingSchedules = res.data.upcoming || [];
          this.ongoingSchedules = res.data.ongoing || [];
          this.completedSchedules = res.data.expired || [];
          this.schedules = [
            ...this.upcomingSchedules,
            ...this.ongoingSchedules,
            ...this.completedSchedules,
            ...(res.data.pendingExpiry || [])
          ];
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  markPendingExpiry(id: number) {
    if (!id) return;
    if (!confirm('Mark this schedule as "Arrived / Pending Expiry"?')) return;
    this.scheduleService.markPendingExpiry(id).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.loadData();
        }
      }
    });
  }

  getStatusBadge(name: string): string {
    const s = (name || '').toLowerCase();
    if (s.includes('scheduled') || s.includes('active')) return 'badge-success';
    if (s.includes('cancel')) return 'badge-error';
    if (s.includes('complet') || s.includes('expire')) return 'badge-info';
    if (s.includes('pending')) return 'badge-warning';
    return 'badge-ghost';
  }
}
