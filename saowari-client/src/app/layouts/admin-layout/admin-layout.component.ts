import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService, NotificationItem } from '../../core/services/api/notification.service';
import { SettingsService } from '../../core/services/api/settings.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="flex h-screen bg-saowari-surface-alt overflow-hidden">
      <!-- Sidebar -->
      <aside [ngClass]="isCollapsed ? 'w-16' : 'w-72'" class="bg-saowari-text-primary text-white flex flex-col transition-all duration-300 relative z-20 shadow-xl animate-fade-in">
        <!-- Logo -->
        <div class="h-16 flex items-center justify-between px-4 border-b border-gray-800">
          <div class="flex items-center gap-3 overflow-hidden h-full py-2">
            <span *ngIf="isCollapsed || !globalLogoUrl" class="text-saowari-accent font-bold text-xl flex-shrink-0">S</span>
            <span *ngIf="!isCollapsed && !globalLogoUrl" class="font-heading font-bold text-xl tracking-wider whitespace-nowrap">SAOWARI</span>
            <img *ngIf="!isCollapsed && globalLogoUrl" [src]="globalLogoUrl" alt="SAOWARI" class="max-h-full max-w-[200px] object-contain animate-fade-in" />
          </div>
          <button (click)="toggleSidebar()" class="text-gray-400 hover:text-white lg:hidden">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- Nav Links -->
        <div class="flex-1 overflow-y-auto py-4 custom-scrollbar">
          <ul class="space-y-1">
            <ng-container *ngFor="let group of filteredMenuGroups">
              <li *ngIf="!isCollapsed" class="px-6 py-2 text-xs font-semibold text-gray-500 uppercase tracking-wider mt-4">{{ group.title }}</li>
              <li *ngIf="isCollapsed && group.title" class="px-4 py-2 mt-2"><div class="h-px bg-gray-800"></div></li>
              
              <li *ngFor="let item of group.items">
                <a [routerLink]="item.path" 
                   routerLinkActive="bg-saowari-accent/15 border-l-4 border-saowari-accent text-white" 
                   [routerLinkActiveOptions]="{exact: item.exact || false}"
                   class="flex items-center px-4 py-3 text-gray-300 hover:bg-gray-800 hover:text-white transition-colors group">
                   <i [class]="item.icon + ' w-6 text-center text-lg'"></i>
                   <span *ngIf="!isCollapsed" class="ml-3 font-medium whitespace-nowrap">{{ item.label }}</span>
                   <!-- Tooltip for collapsed mode -->
                   <div *ngIf="isCollapsed" class="absolute left-16 bg-gray-800 text-white text-xs px-2 py-1 rounded opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity z-50 whitespace-nowrap">
                     {{ item.label }}
                   </div>
                </a>
              </li>
            </ng-container>
          </ul>
        </div>

        <!-- User Profile (Bottom) -->
        <div class="p-4 border-t border-gray-800">
          <div class="flex items-center gap-3">
            <div class="avatar cursor-pointer" [ngClass]="{'placeholder': !currentUser?.picture}" (click)="expandPicture(currentUser?.picture)">
              <div *ngIf="currentUser?.picture" class="w-10 h-10 rounded-full flex-shrink-0 overflow-hidden border border-gray-600 hover:scale-105 transition-transform">
                <img [src]="getProfilePictureUrl(currentUser?.picture)" class="w-full h-full object-cover" />
              </div>
              <div *ngIf="!currentUser?.picture" class="w-10 h-10 rounded-full bg-saowari-primary flex items-center justify-center flex-shrink-0">
                <i class="fas fa-user-shield"></i>
              </div>
            </div>
            <div *ngIf="!isCollapsed" class="overflow-hidden">
              <p class="text-sm font-medium text-white truncate">{{ currentUser?.fullName }}</p>
              <p class="text-xs text-gray-400 truncate">{{ currentUser?.roleName }}</p>
            </div>
          </div>
        </div>
      </aside>

      <!-- Main Content -->
      <div class="flex-1 flex flex-col relative overflow-hidden">
        <!-- Top Header -->
        <header [ngClass]="isDarkMode ? 'bg-slate-900 border-b border-slate-800 text-white' : 'bg-white text-slate-800'" class="h-16 shadow-sm flex items-center justify-between px-6 z-10 transition-colors duration-200">
          <div class="flex items-center gap-4">
            <button (click)="toggleSidebar()" class="text-gray-500 hover:text-saowari-primary transition-colors">
              <i class="fas fa-bars text-xl"></i>
            </button>
            <h2 class="text-xl font-heading font-semibold hidden sm:block">Admin Panel</h2>
          </div>

          <div class="flex items-center gap-6">
            <!-- Global Theme Switcher Button -->
            <button (click)="toggleTheme()" class="p-2 hover:bg-black/5 dark:hover:bg-white/5 rounded-full transition-colors" title="Toggle Light/Dark Theme">
              <svg *ngIf="isDarkMode" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-amber-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 9h-1m15.364-6.364l-.707.707M6.343 17.657l-.707.707m0-12.728l.707.707m12.728 12.728l.707-.707M12 8a4 4 0 100 8 4 4 0 000-8z" />
              </svg>
              <svg *ngIf="!isDarkMode" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
              </svg>
            </button>

            <!-- Notifications Dropdown -->
            <div class="dropdown dropdown-end">
              <label tabindex="0" class="btn btn-ghost btn-circle relative text-gray-500 hover:text-saowari-primary transition-colors">
                <i class="far fa-bell text-xl"></i>
                <span *ngIf="unreadNotificationCount > 0"
                      class="absolute top-1 right-1 w-4 h-4 bg-saowari-danger rounded-full text-[10px] text-white flex items-center justify-center font-bold border-2 border-white animate-fade-in">{{ unreadNotificationCount }}</span>
              </label>
              <ul tabindex="0" class="dropdown-content z-[1] menu p-2 shadow-xl bg-white border border-gray-100 rounded-box w-80 mt-2 text-slate-800">
                <li class="menu-title px-4 py-3 flex flex-row items-center justify-between">
                  <span class="font-bold text-gray-800">Notifications</span>
                  <span *ngIf="unreadNotificationCount > 0" class="badge badge-primary badge-sm">{{ unreadNotificationCount }} new</span>
                </li>
                <div class="divider my-0"></div>
                <li *ngFor="let notif of recentNotifications">
                  <a class="flex items-start gap-3 p-3 hover:bg-gray-50 rounded-lg" [class.opacity-60]="notif.isRead">
                    <div class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 mt-1" [ngClass]="notif.colorClass">
                      <i [class]="notif.icon + ' text-xs'"></i>
                    </div>
                    <div class="flex-1">
                      <p class="text-sm font-semibold text-gray-800" [class.font-normal]="notif.isRead">{{ notif.title }}</p>
                      <p class="text-xs text-gray-500 mt-0.5 line-clamp-1">{{ notif.message }}</p>
                      <p class="text-[10px] text-gray-400 mt-1">{{ timeAgo(notif.createdAt) }}</p>
                    </div>
                  </a>
                </li>
                <li *ngIf="recentNotifications.length === 0">
                  <div class="p-4 text-center text-sm text-gray-400">No notifications</div>
                </li>
                <div class="divider my-0"></div>
                <div class="px-2 pb-1 pt-2 text-center">
                  <a routerLink="/admin/notifications" class="text-xs font-semibold text-saowari-primary hover:underline cursor-pointer block">View all notifications</a>
                </div>
              </ul>
            </div>

            <!-- Profile Picture dropdown -->
            <div class="dropdown dropdown-end">
              <label tabindex="0" class="cursor-pointer flex items-center gap-2">
                <div class="avatar" [ngClass]="{'placeholder': !currentUser?.picture}">
                  <div *ngIf="currentUser?.picture" class="w-8 h-8 rounded-full overflow-hidden border border-gray-200">
                    <img [src]="getProfilePictureUrl(currentUser?.picture)" alt="Avatar" class="w-full h-full object-cover"/>
                  </div>
                  <div *ngIf="!currentUser?.picture" class="w-8 h-8 rounded-full bg-saowari-primary-light text-saowari-primary flex items-center justify-center font-bold">
                    {{ currentUser?.fullName?.charAt(0) || 'A' }}
                  </div>
                </div>
              </label>
              <ul tabindex="0" class="dropdown-content z-[1] menu p-2 shadow bg-base-100 rounded-box w-52 mt-4 text-slate-800">
                <li><a routerLink="/"><i class="fas fa-home mr-2"></i> Go to Website</a></li>
                <div class="divider my-0"></div>
                <li><a (click)="logout()" class="text-saowari-danger"><i class="fas fa-sign-out-alt mr-2"></i> Logout</a></li>
              </ul>
            </div>
          </div>
        </header>

        <!-- Main Scrollable Area -->
        <main [ngClass]="isDarkMode ? 'bg-slate-950 text-slate-100' : 'bg-saowari-surface-alt text-slate-800'" class="flex-1 overflow-y-auto p-6 custom-scrollbar transition-colors duration-200">
          <router-outlet></router-outlet>
        </main>
      </div>
      
      <!-- Mobile Sidebar Overlay -->
      <div *ngIf="!isCollapsed" (click)="toggleSidebar()" class="fixed inset-0 bg-black/50 z-10 lg:hidden"></div>
    </div>

    <!-- Expanded Picture Backdrop Modal -->
    <div 
      *ngIf="expandedPictureUrl" 
      (click)="closeExpandedPicture()"
      class="fixed inset-0 bg-black/90 backdrop-blur-md z-[99999] flex items-center justify-center p-4 cursor-zoom-out animate-fade-in">
      <div class="relative max-w-3xl max-h-[85vh] overflow-hidden rounded-2xl border border-white/10 shadow-2xl animate-scale-up" (click)="$event.stopPropagation()">
        <!-- Close Button -->
        <button 
          (click)="closeExpandedPicture()" 
          class="absolute top-4 right-4 bg-black/60 hover:bg-black/80 text-white rounded-full p-2.5 transition-colors focus:outline-none z-10">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        <img [src]="expandedPictureUrl" alt="Profile Picture Expanded" class="max-w-full max-h-[80vh] object-contain rounded-xl select-none" />
      </div>
    </div>
  `,
  styles: [`
    .custom-scrollbar::-webkit-scrollbar { width: 6px; }
    .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
    .custom-scrollbar::-webkit-scrollbar-thumb { background: #4a6080; border-radius: 3px; }
    .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3372ad; }
    
    .animate-fade-in {
      animation: fadeIn 0.25s ease-out forwards;
    }
    .animate-scale-up {
      animation: scaleUp 0.25s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
    }
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
    @keyframes scaleUp {
      from { transform: scale(0.9); opacity: 0; }
      to { transform: scale(1); opacity: 1; }
    }
  `]
})
export class AdminLayoutComponent implements OnInit {
  isCollapsed = false;
  isDarkMode = false;
  expandedPictureUrl: string | null = null;
  currentUser: any = null;
  unreadNotificationCount = 0;

  menuGroups: any[] = [];
  globalSearchTerm: string = '';

  recentNotifications: NotificationItem[] = [];
  globalLogoUrl: string | null = null;

  constructor(
    public authService: AuthService, 
    public notificationService: NotificationService,
    private settingsService: SettingsService
  ) {}

  get filteredMenuGroups() {
    if (!this.globalSearchTerm || this.globalSearchTerm.trim() === '') {
      return this.menuGroups;
    }
    const term = this.globalSearchTerm.toLowerCase();
    
    return this.menuGroups.map(group => {
      const filteredItems = group.items.filter((item: any) => 
        item.label.toLowerCase().includes(term) || (group.title && group.title.toLowerCase().includes(term))
      );
      return { ...group, items: filteredItems };
    }).filter(group => group.items.length > 0);
  }

  ngOnInit() {
    if (window.innerWidth < 1024) {
      this.isCollapsed = true;
    }
    
    // Resolve saved theme
    const savedTheme = localStorage.getItem('admin_theme');
    this.isDarkMode = savedTheme ? savedTheme === 'dark' : false;
    this.applyTheme();

    // Listen to current user
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.buildMenu();
    });

    // Load real notifications for the dropdown
    this.notificationService.getAll().subscribe((res: any) => {
      if (res.success) {
        this.recentNotifications = res.data.slice(0, 3);
      }
    });

    // Keep unread notification count updated
    this.notificationService.unreadCount$.subscribe(cnt => {
      this.unreadNotificationCount = cnt;
    });

    // Load global logo
    this.settingsService.getLogo().subscribe((res: any) => {
      if (res.success && res.data) {
        this.globalLogoUrl = res.data;
      }
    });
  }

  buildMenu() {
    this.menuGroups = [];

    if (this.authService.isAdmin() || this.authService.isAgent()) {
      this.menuGroups = [
        {
          title: 'Overview',
          items: [
            { label: 'Dashboard', path: '/admin/dashboard', icon: 'fas fa-chart-pie', exact: true },
            { label: 'Messenger', path: '/admin/messenger', icon: 'fas fa-comments' }
          ]
        },
        {
          title: 'Management',
          items: [
            { label: 'User Roles', path: '/admin/roles', icon: 'fas fa-users-cog' },
            { label: 'Users', path: '/admin/users', icon: 'fas fa-users' },
            { label: 'Companies', path: '/admin/companies', icon: 'fas fa-building' },
            { label: 'Locations', path: '/admin/locations', icon: 'fas fa-map-marker-alt' },
            { label: 'Slider Images', path: '/admin/sliders', icon: 'fas fa-images' },
            { label: 'Home Banners', path: '/admin/banners', icon: 'fas fa-bullhorn' },
            { label: 'Vehicles', path: '/admin/vehicles', icon: 'fas fa-bus' },
            { label: 'Seat Classes', path: '/admin/seat-classes', icon: 'fas fa-chair' },
            { label: 'Routes', path: '/admin/routes', icon: 'fas fa-route' },
            { label: 'Schedules', path: '/admin/schedules', icon: 'far fa-calendar-alt' },
            { label: 'Schedule Lifecycle', path: '/admin/schedule-lifecycle', icon: 'fas fa-history' }
          ]
        },
        {
          title: 'Transactions',
          items: [
            { label: 'Bookings', path: '/admin/bookings', icon: 'fas fa-clipboard-list' },
            { label: 'Payments', path: '/admin/payments', icon: 'fas fa-credit-card' },
            { label: 'Payment Methods', path: '/admin/payment-methods', icon: 'fas fa-cogs' },
            { label: 'Refunds', path: '/admin/refunds', icon: 'fas fa-undo-alt' },
            { label: 'Discounts', path: '/admin/discounts', icon: 'fas fa-tags' }
          ]
        },
        {
          title: 'Analytics',
          items: [
            { label: 'Reports', path: '/admin/reports', icon: 'fas fa-chart-line' }
          ]
        },
        {
          title: 'System',
          items: [
            { label: 'Settings', path: '/admin/settings', icon: 'fas fa-cogs' }
          ]
        }
      ];
    } else if (this.authService.isCompanyManager()) {
      this.menuGroups = [
        {
          title: 'Overview',
          items: [
            { label: 'Dashboard', path: '/admin/manager-dashboard', icon: 'fas fa-chart-pie', exact: true }
          ]
        },
        {
          title: 'Management',
          items: [
            { label: 'My Vehicles', path: '/admin/vehicles', icon: 'fas fa-bus' },
            { label: 'Routes', path: '/admin/routes', icon: 'fas fa-route' },
            { label: 'My Schedules', path: '/admin/schedules', icon: 'far fa-calendar-alt' },
            { label: 'Schedule Lifecycle', path: '/admin/schedule-lifecycle', icon: 'fas fa-history' }
          ]
        },
        {
          title: 'Transactions',
          items: [
            { label: 'Company Refunds', path: '/admin/refunds', icon: 'fas fa-undo-alt' }
          ]
        },
        {
          title: 'Analytics',
          items: [
            { label: 'Reports', path: '/admin/reports', icon: 'fas fa-chart-line' }
          ]
        }
      ];
    } else if (this.authService.isSupervisor()) {
      this.menuGroups = [
        {
          title: 'Operations',
          items: [
            { label: 'Dashboard', path: '/admin/supervisor-dashboard', icon: 'fas fa-chart-pie', exact: true }
          ]
        },
        {
          title: 'Management',
          items: [
            { label: 'Schedule Lifecycle', path: '/admin/schedule-lifecycle', icon: 'fas fa-history' }
          ]
        }
      ];
    }
  }

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }

  logout() {
    this.authService.logout();
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
    localStorage.setItem('admin_theme', this.isDarkMode ? 'dark' : 'light');
    this.applyTheme();
    // Emit global event to update children pages
    window.dispatchEvent(new CustomEvent('admin-theme-changed', { detail: { isDarkMode: this.isDarkMode } }));
  }

  applyTheme() {
    const body = document.body;
    if (this.isDarkMode) {
      body.classList.add('dark-mode-active');
    } else {
      body.classList.remove('dark-mode-active');
    }
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
