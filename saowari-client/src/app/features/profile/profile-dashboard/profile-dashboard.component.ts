import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { BookingService } from '../../../core/services/api/booking.service';
import { UserModel } from '../../../core/models/auth.model';

@Component({
  selector: 'app-profile-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './profile-dashboard.component.html',
  styleUrls: ['./profile-dashboard.component.css']
})
export class ProfileDashboardComponent implements OnInit {
  currentUser: UserModel | null = null;
  totalBookings = 0;
  upcomingBookings = 0;
  completedBookings = 0;
  cancelledBookings = 0;
  totalSpent = 0;
  isLoading = true;
  expandedPictureUrl: string | null = null;

  quickLinks = [
    { label: 'My Bookings', icon: 'fas fa-clipboard-list', route: '/profile/my-bookings', color: 'bg-blue-50 text-blue-600' },
    { label: 'My Tickets', icon: 'fas fa-ticket-alt', route: '/profile/my-tickets', color: 'bg-green-50 text-green-600' },
    { label: 'My Refunds', icon: 'fas fa-undo-alt', route: '/profile/my-refunds', color: 'bg-orange-50 text-orange-600' },
    { label: 'Schedule Chats', icon: 'fas fa-comments', route: '/profile/schedule-chats', color: 'bg-emerald-50 text-emerald-650' },
    { label: 'Edit Profile', icon: 'fas fa-user-edit', route: '/profile/edit-profile', color: 'bg-purple-50 text-purple-600' },
    { label: 'Change Password', icon: 'fas fa-lock', route: '/profile/change-password', color: 'bg-red-50 text-red-600' },
    { label: 'Search Trips', icon: 'fas fa-search', route: '/home', color: 'bg-saowari-primary-light text-saowari-primary' }
  ];

  constructor(
    private authService: AuthService,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    this.loadStats();
  }

  loadStats() {
    this.isLoading = true;
    this.bookingService.getMy().subscribe({
      next: (res: any) => {
        if (res.success) {
          const bookings = res.data || [];
          this.totalBookings = bookings.length;
          const now = new Date();
          this.upcomingBookings = bookings.filter((b: any) => new Date(b.departureDateTime) >= now && b.bookingStatus !== 'Cancelled').length;
          this.completedBookings = bookings.filter((b: any) => new Date(b.departureDateTime) < now && b.bookingStatus !== 'Cancelled').length;
          this.cancelledBookings = bookings.filter((b: any) => b.bookingStatus === 'Cancelled').length;
          this.totalSpent = bookings.filter((b: any) => b.bookingStatus !== 'Cancelled').reduce((sum: number, b: any) => sum + (b.totalAmount || b.finalAmount || 0), 0);
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  getProfilePictureUrl(path: string | null | undefined): string {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('data:')) {
      return path;
    }
    const cleanPath = path.startsWith('/') ? path : '/' + path;
    return 'http://localhost:5293' + cleanPath;
  }

  expandPicture(url: string | null | undefined): void {
    if (url) {
      this.expandedPictureUrl = this.getProfilePictureUrl(url);
    }
  }

  closeExpandedPicture(): void {
    this.expandedPictureUrl = null;
  }

  getInitials(): string {
    if (!this.currentUser?.fullName) return 'U';
    return this.currentUser.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
