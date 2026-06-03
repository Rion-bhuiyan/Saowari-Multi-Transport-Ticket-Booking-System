import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { AuthService } from '../../../../core/services/auth.service';
import { BookingService } from '../../../../core/services/api/booking.service';

@Component({
  selector: 'app-driver-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './driver-dashboard.component.html',
  styleUrls: ['./driver-dashboard.component.css']
})
export class DriverDashboardComponent implements OnInit {
  isLoading = true;
  upcomingSchedules: any[] = [];
  ongoingSchedules: any[] = [];
  nextTrip: any = null;
  currentUser: any = null;
  totalPassengers = 0;
  
  constructor(
    private scheduleService: ScheduleService,
    private authService: AuthService,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.scheduleService.getLifecycle().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.upcomingSchedules = res.data.upcoming || [];
          this.ongoingSchedules = res.data.ongoing || [];
          
          if (this.ongoingSchedules.length > 0) {
            this.nextTrip = this.ongoingSchedules[0];
          } else if (this.upcomingSchedules.length > 0) {
            this.nextTrip = this.upcomingSchedules[0];
          }

          if (this.nextTrip) {
            this.loadTripBookings(this.nextTrip.scheduleID || this.nextTrip.scheduleId);
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

  loadTripBookings(scheduleId: number) {
    this.bookingService.getBySchedule(scheduleId).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          const bookings = res.data;
          this.totalPassengers = bookings.reduce((sum: number, b: any) => sum + (b.bookingSeats?.length || 1), 0);
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  getStatusBadge(name: string): string {
    const s = (name || '').toLowerCase();
    if (s.includes('scheduled') || s.includes('active')) return 'bg-blue-500/20 text-blue-300 border border-blue-500/50';
    if (s.includes('cancel')) return 'bg-red-500/20 text-red-300 border border-red-500/50';
    if (s.includes('complet')) return 'bg-green-500/20 text-green-300 border border-green-500/50';
    return 'bg-yellow-500/20 text-yellow-300 border border-yellow-500/50';
  }
}
