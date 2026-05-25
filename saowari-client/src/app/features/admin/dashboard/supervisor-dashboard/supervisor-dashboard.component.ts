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
  
  constructor(private scheduleService: ScheduleService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.scheduleService.getAll().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.schedules = res.data;
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
    if (s.includes('scheduled') || s.includes('active')) return 'badge-success';
    if (s.includes('cancel')) return 'badge-error';
    if (s.includes('complet')) return 'badge-info';
    return 'badge-warning';
  }
}
