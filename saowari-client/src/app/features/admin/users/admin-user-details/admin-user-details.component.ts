import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { UserService } from '../../../../core/services/api/user.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-user-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-user-details.component.html',
  styleUrls: ['./admin-user-details.component.css']
})
export class AdminUserDetailsComponent implements OnInit {
  userId!: number;
  profile: any = null;
  isLoading = true;
  activeTab = 'overview';
  
  timeline: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.userId = +id;
        this.loadProfile();
      }
    });
  }

  loadProfile() {
    this.isLoading = true;
    this.userService.getAdminUserProfile(this.userId).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.profile = res.data;
          this.buildTimeline();
        } else {
          this.notification.error('Failed to load user profile');
        }
        this.isLoading = false;
      },
      error: () => {
        this.notification.error('Error loading user profile');
        this.isLoading = false;
      }
    });
  }

  buildTimeline() {
    const events: any[] = [];

    // Add account creation
    if (this.profile.createdAt) {
      events.push({
        type: 'joined',
        date: new Date(this.profile.createdAt),
        title: 'Account Created',
        desc: `Joined as ${this.profile.roleName || 'User'}`,
        icon: 'user-plus'
      });
    }

    // Add logins
    if (this.profile.loginHistory && this.profile.loginHistory.length > 0) {
      this.profile.loginHistory.forEach((l: any) => {
        events.push({
          type: 'login',
          date: new Date(l.loginTime),
          title: 'Logged In',
          desc: `from ${l.location || 'Unknown location'} via ${l.deviceName.split(' ')[0] || 'Device'}`,
          icon: 'log-in',
          ip: l.ipAddress
        });
      });
    }

    // Add bookings
    if (this.profile.bookings && this.profile.bookings.length > 0) {
      this.profile.bookings.forEach((b: any) => {
        events.push({
          type: 'booking',
          date: new Date(b.bookingDate),
          title: 'Purchased Ticket',
          desc: `Booking #${b.bookingCode} for ${b.routeName || 'a trip'}`,
          amount: b.finalAmount,
          icon: 'ticket',
          bus: b.vehicleName,
          busId: b.vehicleId
        });
      });
    }

    // Sort descending
    this.timeline = events.sort((a, b) => b.date.getTime() - a.date.getTime());
  }

  setTab(tab: string) {
    this.activeTab = tab;
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.split(' ');
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  }

  navigateToBus(vehicleId: number | undefined | null) {
    if (vehicleId) {
      this.router.navigate(['/admin/vehicles']); 
    }
  }

  navigateToSchedule(scheduleId: number | undefined | null) {
    if (scheduleId) {
      this.router.navigate(['/admin/schedules', scheduleId, 'seat-map']);
    }
  }

  parseUserAgent(ua: string): string {
    if (!ua) return 'Unknown Device';
    let browser = 'Unknown Browser';
    let os = 'Unknown OS';

    // Basic Browser Detection
    if (ua.includes('Firefox/')) browser = 'Firefox';
    else if (ua.includes('Edg/')) browser = 'Edge';
    else if (ua.includes('Chrome/')) browser = 'Chrome';
    else if (ua.includes('Safari/') && !ua.includes('Chrome')) browser = 'Safari';

    // Basic OS Detection
    if (ua.includes('Windows NT 10.0')) os = 'Windows 10/11';
    else if (ua.includes('Windows NT')) os = 'Windows';
    else if (ua.includes('Mac OS X')) os = 'macOS';
    else if (ua.includes('Android')) os = 'Android';
    else if (ua.includes('iPhone') || ua.includes('iPad')) os = 'iOS';
    else if (ua.includes('Linux')) os = 'Linux';

    if (browser === 'Unknown Browser' && os === 'Unknown OS') return ua;
    return `${browser} on ${os}`;
  }
}
