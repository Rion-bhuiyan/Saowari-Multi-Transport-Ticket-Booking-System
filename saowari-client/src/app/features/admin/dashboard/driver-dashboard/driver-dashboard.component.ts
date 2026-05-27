import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { AuthService } from '../../../../core/services/auth.service';

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
  
  constructor(
    private scheduleService: ScheduleService,
    private authService: AuthService
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
    if (s.includes('scheduled') || s.includes('active')) return 'bg-blue-100 text-blue-800 border-blue-200';
    if (s.includes('cancel')) return 'bg-red-100 text-red-800 border-red-200';
    if (s.includes('complet')) return 'bg-green-100 text-green-800 border-green-200';
    return 'bg-yellow-100 text-yellow-800 border-yellow-200';
  }
}
