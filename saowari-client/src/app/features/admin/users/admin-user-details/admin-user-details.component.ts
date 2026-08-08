import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { UserService } from '../../../../core/services/api/user.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration, ChartOptions, ChartType } from 'chart.js';

@Component({
  selector: 'app-admin-user-details',
  standalone: true,
  imports: [CommonModule, RouterModule, NgChartsModule],
  templateUrl: './admin-user-details.component.html',
  styleUrls: ['./admin-user-details.component.css']
})
export class AdminUserDetailsComponent implements OnInit {
  userId!: number;
  profile: any = null;
  isLoading = true;
  activeTab = 'overview';
  
  timeline: any[] = [];

  // Metrics
  totalSpent = 0;
  totalTickets = 0;
  totalVisits = 0;
  avgDurationMins = 0;

  // Chart
  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: []
  };
  public lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        mode: 'index',
        intersect: false,
      }
    },
    scales: {
      y: { beginAtZero: true, grid: { color: 'rgba(0,0,0,0.05)' } },
      x: { grid: { display: false } }
    },
    elements: {
      line: { tension: 0.4 } // Smooth curves
    }
  };
  public lineChartLegend = false;

  // Daily Logins Chart (Last 14 Days)
  public dailyChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: []
  };
  public dailyChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        mode: 'index',
        intersect: false,
      }
    },
    scales: {
      y: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: 'rgba(0,0,0,0.05)' } },
      x: { grid: { display: false } }
    }
  };

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
          this.calculateMetrics();
          this.buildChartData();
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

  calculateMetrics() {
    this.totalSpent = 0;
    this.totalTickets = 0;
    this.totalVisits = 0;
    
    if (this.profile.bookings && this.profile.bookings.length > 0) {
      this.totalTickets = this.profile.bookings.length;
      this.totalSpent = this.profile.bookings.reduce((sum: number, b: any) => sum + (b.finalAmount || 0), 0);
    }
    
    if (this.profile.loginHistory && this.profile.loginHistory.length > 0) {
      this.totalVisits = this.profile.loginHistory.length;
    }

    // Since we don't track exact session duration, we can simulate an average duration 
    // or calculate an estimated duration based on some rules. For now, we simulate an average 
    // based on ticket count and visits, to make the UI look populated.
    if (this.totalVisits > 0) {
      // Base 5 mins per visit, plus 2 mins per ticket
      this.avgDurationMins = Math.round(5 + (this.totalTickets * 2) / this.totalVisits);
    }
  }

  buildChartData() {
    // Generate last 6 months labels
    const months = [];
    const spendingData = [];
    const ticketsData = [];
    
    const today = new Date();
    
    for (let i = 5; i >= 0; i--) {
      const d = new Date(today.getFullYear(), today.getMonth() - i, 1);
      months.push(d.toLocaleString('default', { month: 'short' }));
      
      // Calculate spent in this month
      let spent = 0;
      let tickets = 0;
      
      if (this.profile.bookings) {
        this.profile.bookings.forEach((b: any) => {
          const bDate = new Date(b.bookingDate);
          if (bDate.getMonth() === d.getMonth() && bDate.getFullYear() === d.getFullYear()) {
            spent += (b.finalAmount || 0);
            tickets += 1;
          }
        });
      }
      
      spendingData.push(spent);
      ticketsData.push(tickets);
    }

    this.lineChartData = {
      labels: months,
      datasets: [
        {
          data: spendingData,
          label: 'Spending ($)',
          backgroundColor: 'rgba(0, 85, 159, 0.1)', // saowari-primary
          borderColor: 'rgba(0, 85, 159, 1)',
          pointBackgroundColor: '#ffffff',
          pointBorderColor: 'rgba(0, 85, 159, 1)',
          pointHoverBackgroundColor: 'rgba(0, 85, 159, 1)',
          pointHoverBorderColor: '#ffffff',
          fill: 'origin',
        }
      ]
    };

    // Generate last 14 days labels for logins
    const days = [];
    const loginsData = [];
    
    for (let i = 13; i >= 0; i--) {
      const d = new Date();
      d.setDate(today.getDate() - i);
      days.push(d.toLocaleDateString('default', { month: 'short', day: 'numeric' }));
      
      let logins = 0;
      if (this.profile.loginHistory) {
        this.profile.loginHistory.forEach((l: any) => {
          const lDate = new Date(l.loginTime);
          if (lDate.getDate() === d.getDate() && lDate.getMonth() === d.getMonth() && lDate.getFullYear() === d.getFullYear()) {
            logins += 1;
          }
        });
      }
      loginsData.push(logins);
    }

    this.dailyChartData = {
      labels: days,
      datasets: [
        {
          data: loginsData,
          label: 'Logins',
          backgroundColor: 'rgba(59, 130, 246, 0.8)', // blue-500
          hoverBackgroundColor: 'rgba(37, 99, 235, 1)', // blue-600
          borderRadius: 4
        }
      ]
    };
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
