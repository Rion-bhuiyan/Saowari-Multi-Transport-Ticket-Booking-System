import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { BookingService } from '../../../../core/services/api/booking.service';
import { BookingModel } from '../../../../core/models/transaction.model';

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

  activeAssignment: any = null;
  passengers: any[] = [];

  today = new Date();

  get totalTrips() { return this.schedules.length; }
  get ongoingCount() { return this.ongoingSchedules.length; }
  get upcomingCount() { return this.upcomingSchedules.length; }
  get completedCount() { return this.completedSchedules.length; }
  
  constructor(
    private scheduleService: ScheduleService,
    private bookingService: BookingService
  ) {}

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

          // Determine active assignment
          if (this.ongoingSchedules.length > 0) {
            this.activeAssignment = this.ongoingSchedules[0];
          } else if (this.upcomingSchedules.length > 0) {
            this.activeAssignment = this.upcomingSchedules[0];
          }

          if (this.activeAssignment) {
            this.loadPassengerManifest(this.activeAssignment.scheduleID || this.activeAssignment.scheduleId);
          } else {
            this.isLoading = false;
          }
        } else {
          this.isLoading = false;
        }
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadPassengerManifest(scheduleId: number) {
    this.bookingService.getBySchedule(scheduleId).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          const bookings = res.data as BookingModel[];
          this.passengers = [];
          
          bookings.forEach(b => {
            b.seats?.forEach((seat: string) => {
              this.passengers.push({
                name: b.passengerName || 'Unknown',
                phone: b.passengerPhone || 'N/A',
                seatNumber: seat,
                status: b.bookingStatusName || 'Confirmed'
              });
            });
          });
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
    if (s.includes('scheduled') || s.includes('active')) return 'bg-green-500/20 text-green-300 border border-green-500/50';
    if (s.includes('cancel')) return 'bg-red-500/20 text-red-300 border border-red-500/50';
    if (s.includes('complet') || s.includes('expire')) return 'bg-blue-500/20 text-blue-300 border border-blue-500/50';
    if (s.includes('pending')) return 'bg-yellow-500/20 text-yellow-300 border border-yellow-500/50';
    return 'bg-gray-500/20 text-gray-300 border border-gray-500/50';
  }
}
