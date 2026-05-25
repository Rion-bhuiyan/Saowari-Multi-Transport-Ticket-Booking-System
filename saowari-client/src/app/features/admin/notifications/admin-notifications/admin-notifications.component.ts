import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationService, NotificationItem, AdminNotificationPreference } from '../../../../core/services/api/notification.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-admin-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-notifications.component.html',
  styleUrls: ['./admin-notifications.component.css']
})
export class AdminNotificationsComponent implements OnInit, OnDestroy {
  activeTab = 'all'; // 'all' | 'unread'
  selectedType = 'all'; // 'all' | 'booking' | 'cancellation' | 'refund' | 'user' | 'system'
  selectedCompany = 'all'; // 'all' | companyName
  
  isLoading = true;
  isSavingPreference = false;
  showPrefsModal = false;
  
  notifications: NotificationItem[] = [];
  preferences: AdminNotificationPreference[] = [];
  
  private pollingIntervalId: any;

  constructor(
    private notificationService: NotificationService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
    this.loadPreferences();

    // Start 30-second real-time polling
    this.pollingIntervalId = setInterval(() => {
      this.refreshNotificationsSilent();
    }, 30000);
  }

  ngOnDestroy(): void {
    if (this.pollingIntervalId) {
      clearInterval(this.pollingIntervalId);
    }
  }

  loadNotifications() {
    this.isLoading = true;
    this.notificationService.getAll().subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.notifications = res.data;
        }
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  refreshNotificationsSilent() {
    this.notificationService.getAll().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notifications = res.data;
        }
      }
    });
  }

  loadPreferences() {
    if (this.authService.isAdmin()) {
      this.notificationService.getAdminPreferences().subscribe({
        next: (res: any) => {
          if (res.success) {
            this.preferences = res.data;
          }
        }
      });
    }
  }

  get unreadCount() {
    return this.notifications.filter(n => !n.isRead).length;
  }

  get totalCount() {
    return this.notifications.length;
  }

  // Get dynamic unique companies list from notifications for filter dropdown
  get companyList() {
    const companies = this.notifications
      .map(n => n.companyName)
      .filter((name): name is string => !!name);
    return Array.from(new Set(companies));
  }

  get filteredNotifications() {
    return this.notifications.filter(n => {
      // 1. Status Filter
      if (this.activeTab === 'unread' && n.isRead) return false;
      
      // 2. Type Filter
      if (this.selectedType !== 'all' && n.type !== this.selectedType) return false;
      
      // 3. Company Filter
      if (this.selectedCompany !== 'all' && n.companyName !== this.selectedCompany) return false;
      
      return true;
    });
  }

  markAllAsRead() {
    if (this.notifications.every(n => n.isRead)) return;
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
      }
    });
  }

  markAsRead(nItem: NotificationItem) {
    if (nItem.isRead) return;
    this.notificationService.markAsRead(nItem.id).subscribe({
      next: () => {
        nItem.isRead = true;
      }
    });
  }

  deleteNotification(id: number, event: Event) {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this notification?')) {
      this.notificationService.delete(id).subscribe({
        next: () => {
          this.notifications = this.notifications.filter(n => n.id !== id);
        }
      });
    }
  }

  clearAll() {
    if (this.notifications.length === 0) return;
    if (confirm('Are you sure you want to permanently clear all your notifications?')) {
      this.isLoading = true;
      this.notificationService.clearAll().subscribe({
        next: () => {
          this.notifications = [];
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
    }
  }

  togglePreference(companyId: number, isCurrentlyEnabled: boolean) {
    this.isSavingPreference = true;
    const newStatus = !isCurrentlyEnabled;
    this.notificationService.togglePreference(companyId, newStatus).subscribe({
      next: (res: any) => {
        this.isSavingPreference = false;
        if (res.success) {
          const pref = this.preferences.find(p => p.companyId === companyId);
          if (pref) {
            pref.isEnabled = newStatus;
          }
        }
      },
      error: () => {
        this.isSavingPreference = false;
      }
    });
  }

  getTypeIcon(type: string): string {
    switch (type) {
      case 'booking': return 'fas fa-ticket-alt';
      case 'cancellation': return 'fas fa-times-circle';
      case 'refund': return 'fas fa-undo-alt';
      case 'user': return 'fas fa-user-plus';
      case 'vehicle': return 'fas fa-bus';
      case 'schedule': return 'fas fa-calendar-alt';
      default: return 'fas fa-bell';
    }
  }

  getTypeColorClass(type: string): string {
    switch (type) {
      case 'booking': return 'bg-emerald-100 text-emerald-600 dark:bg-emerald-950 dark:text-emerald-400';
      case 'cancellation': return 'bg-rose-100 text-rose-600 dark:bg-rose-950 dark:text-rose-400';
      case 'refund': return 'bg-amber-100 text-amber-600 dark:bg-amber-950 dark:text-amber-400';
      case 'user': return 'bg-indigo-100 text-indigo-600 dark:bg-indigo-950 dark:text-indigo-400';
      case 'vehicle': return 'bg-teal-100 text-teal-600 dark:bg-teal-950 dark:text-teal-400';
      case 'schedule': return 'bg-sky-100 text-sky-600 dark:bg-sky-950 dark:text-sky-400';
      default: return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400';
    }
  }

  timeAgo(dateStr: string): string {
    if (!dateStr) return '';
    if (!dateStr.endsWith('Z') && !dateStr.includes('+')) dateStr += 'Z';
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} mins ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays === 1) return 'Yesterday';
    return `${diffDays} days ago`;
  }
}
