import { Component, OnInit, OnDestroy, NgZone, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScheduleService } from '../../../../core/services/api/schedule.service';
import { CompanyService } from '../../../../core/services/api/company.service';
import { UserService } from '../../../../core/services/api/user.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-schedule-lifecycle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './schedule-lifecycle.component.html'
})
export class ScheduleLifecycleComponent implements OnInit, OnDestroy {
  activeTab: 'upcoming' | 'ongoing' | 'pendingExpiry' | 'expired' = 'upcoming';
  
  upcoming: any[] = [];
  ongoing: any[] = [];
  pendingExpiry: any[] = [];
  expired: any[] = [];
  
  companies: any[] = [];
  selectedCompanyId: number | null = null;
  
  driversList: any[] = [];
  supervisorsList: any[] = [];

  isLoading = true;
  timerInterval: any;
  now = new Date();

  // Clone Modal State
  isCloneModalOpen = false;
  cloningItem: any = null;
  cloneModel: any = {
    driverInformtionId: null,
    supervisorId: null,
    departureTime: '',
    arrivalTime: '',
    seatClassPricings: []
  };

  constructor(
    private scheduleService: ScheduleService,
    private companyService: CompanyService,
    private userService: UserService,
    private notification: NotificationService,
    public authService: AuthService,
    private router: Router,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadData();
    this.loadUsers();
    if (this.authService.isAdmin()) {
      this.companyService.getAll().subscribe((res: any) => {
        if (res.success) this.companies = res.data || [];
      });
    }

    // Timer for live countdowns optimized outside Angular zone to prevent global refresh jitter
    this.ngZone.runOutsideAngular(() => {
      this.timerInterval = setInterval(() => {
        this.now = new Date();
        this.cdr.detectChanges(); // Only refresh this specific component's UI
      }, 1000);
    });
  }

  ngOnDestroy() {
    if (this.timerInterval) clearInterval(this.timerInterval);
  }

  loadData() {
    this.isLoading = true;
    this.scheduleService.getLifecycle(this.selectedCompanyId || undefined).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.upcoming = res.data.upcoming || [];
          this.ongoing = res.data.ongoing || [];
          this.pendingExpiry = res.data.pendingExpiry || [];
          this.expired = res.data.expired || [];
        }
        this.isLoading = false;
      },
      error: () => {
        this.notification.error('Failed to load lifecycle data');
        this.isLoading = false;
      }
    });
  }

  loadUsers() {
    this.userService.getAll().subscribe((res: any) => {
      if (res.success) {
        const users = res.data || [];
        this.driversList = users.filter((u: any) =>
          (u.roleName || u.RoleName || '').toLowerCase() === 'driver' &&
          (u.driverInformtionId || u.DriverInformtionId)
        );
        this.supervisorsList = users.filter((u: any) =>
          (u.roleName || u.RoleName || '').toLowerCase() === 'supervisor' &&
          (u.supervisorId || u.SupervisorId)
        );
      }
    });
  }

  onCompanyChange() {
    this.loadData();
  }

  setTab(tab: 'upcoming' | 'ongoing' | 'pendingExpiry' | 'expired') {
    this.activeTab = tab;
  }

  markPending(id: number) {
    if (confirm('Mark this schedule as Pending Expiry for review?')) {
      this.scheduleService.markPendingExpiry(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Schedule marked for expiry review');
            this.loadData();
          } else {
            this.notification.error(res.message || 'Failed to mark pending');
          }
        },
        error: () => this.notification.error('Error marking pending')
      });
    }
  }

  approveExpiry(id: number) {
    if (confirm('Approve expiration? This will move it to the archive.')) {
      this.scheduleService.approveExpiry(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Schedule officially expired');
            this.loadData();
          } else {
            this.notification.error(res.message || 'Failed to approve expiry');
          }
        },
        error: () => this.notification.error('Error approving expiry')
      });
    }
  }

  openCloneModal(item: any) {
    this.cloningItem = item;
    this.cloneModel = {
      driverInformtionId: null,
      supervisorId: null,
      departureTime: '',
      arrivalTime: '',
      seatClassPricings: item.seatClassPricings ? JSON.parse(JSON.stringify(item.seatClassPricings)) : []
    };
    this.isCloneModalOpen = true;
  }

  closeCloneModal() {
    this.isCloneModalOpen = false;
    this.cloningItem = null;
  }

  saveClone() {
    if (!this.cloneModel.driverInformtionId || !this.cloneModel.departureTime || !this.cloneModel.arrivalTime) {
      this.notification.error('Driver and Dates are required.');
      return;
    }
    if (new Date(this.cloneModel.arrivalTime) <= new Date(this.cloneModel.departureTime)) {
      this.notification.error('Arrival time must be after departure time.');
      return;
    }

    const payload = {
      originalScheduleId: this.cloningItem.scheduleID || this.cloningItem.scheduleId,
      driverInformtionId: Number(this.cloneModel.driverInformtionId),
      supervisorId: this.cloneModel.supervisorId ? Number(this.cloneModel.supervisorId) : null,
      departureDateTime: this.cloneModel.departureTime,
      arrivalDateTime: this.cloneModel.arrivalTime,
      seatClassPricings: this.cloneModel.seatClassPricings.map((p: any) => ({
        seatClassId: p.seatClassId,
        price: Number(p.price)
      }))
    };

    this.scheduleService.cloneSchedule(payload).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Schedule cloned and made live successfully!');
          this.closeCloneModal();
          this.loadData();
          this.setTab('upcoming');
        } else {
          this.notification.error(res.message || 'Failed to clone');
        }
      },
      error: (err: any) => {
        this.notification.error(err?.error?.message || 'Error cloning schedule');
      }
    });
  }

  goToDetails(id: number) {
    if (id) {
      this.router.navigate(['/admin/schedules', id, 'seat-map']);
    }
  }

  getCountdown(dateStr: string, isArrival = false): string {
    if (!dateStr) return '';
    const targetDate = new Date(dateStr).getTime();
    const nowTime = this.now.getTime();
    
    let diffMs = isArrival ? targetDate - nowTime : targetDate - nowTime;
    
    if (diffMs <= 0) {
      return isArrival ? 'Arrived' : 'Departed';
    }

    const d = Math.floor(diffMs / 86400000);
    const h = Math.floor((diffMs % 86400000) / 3600000);
    const m = Math.floor((diffMs % 3600000) / 60000);
    const s = Math.floor((diffMs % 60000) / 1000);

    if (d > 0) {
      return `${d}d ${h}h ${m}m`;
    }

    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  }
}
