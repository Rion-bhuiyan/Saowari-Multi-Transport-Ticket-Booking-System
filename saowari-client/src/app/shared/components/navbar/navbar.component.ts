import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SettingsService } from '../../../core/services/api/settings.service';
import { NotificationService, NotificationItem } from '../../../core/services/api/notification.service';
import { Observable } from 'rxjs';
import { UserModel } from '../../../core/models/auth.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="fixed top-0 left-0 w-full z-[100] transition-all duration-500"
         [ngClass]="isHome && isTransparent ? 'bg-saowari-primary/30 backdrop-blur-md shadow-sm' : 'bg-saowari-primary shadow-lg'">
      <div class="container mx-auto px-4">
        <div class="flex justify-between items-center h-20">
          
          <!-- Logo -->
          <a routerLink="/" class="flex items-center gap-2.5 h-12">
            <img *ngIf="globalLogoUrl" [src]="globalLogoUrl" alt="SAOWARI Logo" class="max-h-full max-w-[50px] object-contain rounded-lg shadow-sm" />
            <span class="text-white font-heading font-extrabold text-2xl tracking-wider uppercase">SAOWARI</span>
          </a>

          <!-- Desktop Menu -->
          <div class="hidden md:flex items-center space-x-8">
            <a routerLink="/home" routerLinkActive="text-saowari-accent" class="text-white hover:text-saowari-accent font-medium transition-colors relative group">
              Home
              <span class="absolute -bottom-1 left-0 w-0 h-0.5 bg-saowari-accent transition-all group-hover:w-full"></span>
            </a>
            <a routerLink="/search" routerLinkActive="text-saowari-accent" class="text-white hover:text-saowari-accent font-medium transition-colors relative group">
              Search Tickets
              <span class="absolute -bottom-1 left-0 w-0 h-0.5 bg-saowari-accent transition-all group-hover:w-full"></span>
            </a>
            <a routerLink="/about" routerLinkActive="text-saowari-accent" class="text-white hover:text-saowari-accent font-medium transition-colors relative group">
              About
              <span class="absolute -bottom-1 left-0 w-0 h-0.5 bg-saowari-accent transition-all group-hover:w-full"></span>
            </a>
            <a routerLink="/contact" routerLinkActive="text-saowari-accent" class="text-white hover:text-saowari-accent font-medium transition-colors relative group">
              Contact
              <span class="absolute -bottom-1 left-0 w-0 h-0.5 bg-saowari-accent transition-all group-hover:w-full"></span>
            </a>
            <a routerLink="/faq" routerLinkActive="text-saowari-accent" class="text-white hover:text-saowari-accent font-medium transition-colors relative group">
              FAQs
              <span class="absolute -bottom-1 left-0 w-0 h-0.5 bg-saowari-accent transition-all group-hover:w-full"></span>
            </a>

            <!-- Theme Switcher -->
            <div class="dropdown dropdown-end">
              <label tabindex="0" class="btn btn-ghost btn-circle relative text-white hover:text-saowari-accent transition-colors" title="Select Theme">
                <i class="fas fa-palette text-xl"></i>
              </label>
              <div tabindex="0" class="dropdown-content z-[100] p-5 shadow-2xl bg-saowari-surface/60 backdrop-blur-2xl border border-saowari-border/50 rounded-3xl w-[320px] mt-3 animate-scale-up">
                <!-- Header -->
                <div class="flex items-center gap-3 mb-4 pb-3 border-b border-saowari-border/50">
                  <div class="w-9 h-9 rounded-xl bg-gradient-hero flex items-center justify-center text-white shadow-md shadow-saowari-primary/20">
                    <i class="fas fa-palette text-sm"></i>
                  </div>
                  <div>
                    <h4 class="font-heading font-extrabold text-xs tracking-wide text-saowari-text-primary">Interface Theme</h4>
                    <p class="text-[9px] text-saowari-text-secondary font-medium">Choose your personal workspace style</p>
                  </div>
                </div>

                <!-- Grid of themes -->
                <div class="grid grid-cols-2 gap-2.5">
                  <button *ngFor="let t of themes" 
                          (click)="setTheme(t.id)"
                          class="flex flex-col items-start p-3 rounded-xl border-2 transition-all duration-300 relative group cursor-pointer text-left w-full select-none"
                          [ngClass]="activeTheme === t.id 
                                     ? 'bg-saowari-surface-alt border-saowari-accent shadow-md shadow-saowari-accent/5' 
                                     : 'bg-saowari-surface/40 border-saowari-border/50 hover:border-saowari-primary hover:bg-saowari-surface-alt hover:-translate-y-0.5'">
                    
                    <!-- Selection Indicator -->
                    <span *ngIf="activeTheme === t.id" 
                          class="absolute top-2 right-2 w-4.5 h-4.5 rounded-full bg-saowari-accent text-white flex items-center justify-center text-[8px] font-bold shadow-sm animate-scale-up border border-saowari-surface">
                      <i class="fas fa-check"></i>
                    </span>

                    <!-- Color Swatch Dots -->
                    <div class="flex gap-1.5 mb-2.5 items-center">
                      <span class="w-3.5 h-3.5 rounded-full border border-black/10 shadow-sm transform transition group-hover:scale-110 duration-200" [style.background]="t.primary" title="Primary"></span>
                      <span class="w-2.5 h-2.5 rounded-full border border-black/10 shadow-sm transform transition group-hover:scale-110 duration-200" [style.background]="t.accent" title="Accent"></span>
                      <span class="w-2.5 h-2.5 rounded-full border border-black/10 shadow-sm transform transition group-hover:scale-110 duration-200" [style.background]="t.surface" title="Surface"></span>
                    </div>

                    <!-- Theme Metadata -->
                    <span class="text-xs font-bold text-saowari-text-primary capitalize tracking-wide transition-colors group-hover:text-saowari-primary">{{ t.name.replace(' mode', '') }}</span>
                    <span class="text-[9px] text-saowari-text-secondary mt-0.5 font-medium leading-tight group-hover:text-saowari-text-primary/70">{{ t.desc }}</span>
                  </button>
                </div>
              </div>
            </div>

            <!-- Auth/Profile Section -->
            <ng-container *ngIf="currentUser === null; else loggedInMenu">
              <a routerLink="/auth/login" class="text-white hover:text-saowari-accent font-medium transition-colors ml-4">Login</a>
              <a routerLink="/auth/register" class="btn btn-outline border-white text-white hover:bg-saowari-surface hover:text-saowari-primary rounded-full px-6 ml-4">Sign Up</a>
            </ng-container>
            
            <ng-template #loggedInMenu>
              <!-- Notifications Dropdown -->
              <div class="dropdown dropdown-end mr-4">
                <label tabindex="0" class="btn btn-ghost btn-circle relative text-white hover:text-saowari-accent transition-colors" (click)="markAllAsRead()">
                  <i class="fas fa-bell text-xl"></i>
                  <span *ngIf="unreadNotificationCount > 0"
                        class="absolute top-1 right-1 w-4 h-4 bg-red-500 rounded-full text-[10px] text-white flex items-center justify-center font-bold border-2 border-saowari-primary animate-fade-in">{{ unreadNotificationCount }}</span>
                </label>
                <ul tabindex="0" class="dropdown-content z-[1] menu p-2 shadow-xl bg-saowari-surface border border-saowari-border rounded-box w-80 mt-2 text-saowari-text-primary">
                  <li class="menu-title px-4 py-3 flex flex-row items-center justify-between border-b border-saowari-border">
                    <span class="font-bold text-saowari-text-primary">Notifications</span>
                    <span *ngIf="unreadNotificationCount > 0" class="badge badge-primary badge-sm">{{ unreadNotificationCount }} new</span>
                  </li>
                  <li *ngFor="let notif of recentNotifications">
                    <a class="flex items-start gap-3 p-3 hover:bg-saowari-surface-alt rounded-lg" [class.opacity-60]="notif.isRead" (click)="markAsRead(notif.id)">
                      <div class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 mt-1" [ngClass]="notif.colorClass">
                        <i [class]="notif.icon + ' text-xs'"></i>
                      </div>
                      <div class="flex-1 overflow-hidden">
                        <p class="text-sm font-semibold text-saowari-text-primary" [class.font-normal]="notif.isRead">{{ notif.title }}</p>
                        <p class="text-xs text-saowari-text-secondary mt-0.5" style="white-space: normal; line-height: 1.2;">{{ notif.message }}</p>
                      </div>
                    </a>
                  </li>
                  <li *ngIf="recentNotifications.length === 0">
                    <div class="p-4 text-center text-sm text-saowari-text-secondary">No notifications</div>
                  </li>
                </ul>
              </div>

              <div class="dropdown dropdown-end">
                <label tabindex="0" class="btn btn-ghost btn-circle avatar border-2 border-white/30 hover:border-white cursor-pointer">
                  <div class="w-10 rounded-full bg-saowari-primary-dark flex items-center justify-center text-white overflow-hidden animate-fade-in">
                    <span *ngIf="!currentUser?.picture" class="text-lg font-bold">{{ getInitials(currentUser?.fullName) }}</span>
                    <img *ngIf="currentUser?.picture" [src]="getProfilePictureUrl(currentUser?.picture)" alt="Avatar" class="w-full h-full object-cover hover:scale-105 transition-transform" />
                  </div>
                </label>
                <ul tabindex="0" class="mt-3 z-[1] p-2 shadow menu menu-sm dropdown-content bg-saowari-surface border border-saowari-border rounded-box w-52 text-saowari-text-primary">
                  <li class="menu-title px-4 py-2 border-b border-saowari-border">
                    <span class="block font-semibold text-saowari-text-primary">{{ currentUser?.fullName }}</span>
                    <span class="block text-xs text-saowari-text-secondary">{{ currentUser?.roleName }}</span>
                  </li>
                  <li *ngIf="isAdmin || isAgent"><a routerLink="/admin/dashboard" class="py-3 hover:bg-saowari-surface-alt"><i class="fas fa-shield-alt mr-2 text-saowari-primary"></i> Admin Panel</a></li>
                  <li *ngIf="isManager"><a routerLink="/admin/manager-dashboard" class="py-3 hover:bg-saowari-surface-alt"><i class="fas fa-building mr-2 text-saowari-primary"></i> Manager Dashboard</a></li>
                  <li *ngIf="isSupervisor"><a routerLink="/admin/supervisor-dashboard" class="py-3 hover:bg-saowari-surface-alt"><i class="fas fa-user-tie mr-2 text-saowari-primary"></i> Supervisor Dashboard</a></li>
                  <li *ngIf="isDriver"><a routerLink="/admin/driver-dashboard" class="py-3 hover:bg-saowari-surface-alt"><i class="fas fa-car mr-2 text-saowari-primary"></i> Driver Dashboard</a></li>
                  <li><a routerLink="/profile/dashboard" class="py-3 hover:bg-saowari-surface-alt"><i class="fas fa-user mr-2 text-saowari-primary"></i> Profile Dashboard</a></li>
                  <li><a routerLink="/profile/my-bookings" class="py-3 hover:bg-saowari-surface-alt"><i class="fas fa-ticket-alt mr-2 text-saowari-primary"></i> My Bookings</a></li>
                  <li><a routerLink="/profile/my-refunds" class="py-3 hover:bg-saowari-surface-alt"><i class="fas fa-undo-alt mr-2 text-saowari-primary"></i> My Refunds</a></li>
                  <div class="divider my-0 bg-saowari-border h-px"></div>
                  <li><a (click)="logout()" class="py-3 text-saowari-danger hover:bg-saowari-surface-alt"><i class="fas fa-sign-out-alt mr-2"></i> Logout</a></li>
                </ul>
              </div>
            </ng-template>
          </div>

          <!-- Mobile Menu Button -->
          <div class="md:hidden flex items-center">
            <button class="btn btn-square btn-ghost text-white" (click)="toggleMobileMenu()">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block w-6 h-6 stroke-current"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path></svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Mobile Menu Dropdown -->
      <div *ngIf="isMobileMenuOpen" class="md:hidden bg-saowari-primary-dark w-full shadow-xl animate-fade-in">
        <ul class="menu w-full p-4 text-white">
          <li><a routerLink="/home" (click)="toggleMobileMenu()">Home</a></li>
          <li><a routerLink="/search" (click)="toggleMobileMenu()">Search Tickets</a></li>
          <li><a routerLink="/about" (click)="toggleMobileMenu()">About Us</a></li>
          <li><a routerLink="/contact" (click)="toggleMobileMenu()">Contact Support</a></li>
          <li><a routerLink="/faq" (click)="toggleMobileMenu()">FAQs</a></li>
          <div class="divider bg-saowari-surface/20 h-px my-2"></div>
          
          <ng-container *ngIf="currentUser === null; else mobileLoggedInMenu">
            <li><a routerLink="/auth/login" (click)="toggleMobileMenu()">Login</a></li>
            <li><a routerLink="/auth/register" (click)="toggleMobileMenu()" class="text-saowari-accent">Sign Up</a></li>
          </ng-container>
          
          <ng-template #mobileLoggedInMenu>
            <li class="menu-title text-gray-300">Welcome, {{ currentUser?.fullName }}</li>
            <li *ngIf="isAdmin || isAgent"><a routerLink="/admin/dashboard" (click)="toggleMobileMenu()">Admin Panel</a></li>
            <li *ngIf="isManager"><a routerLink="/admin/manager-dashboard" (click)="toggleMobileMenu()">Manager Dashboard</a></li>
            <li *ngIf="isSupervisor"><a routerLink="/admin/supervisor-dashboard" (click)="toggleMobileMenu()">Supervisor Dashboard</a></li>
            <li *ngIf="isDriver"><a routerLink="/admin/driver-dashboard" (click)="toggleMobileMenu()">Driver Dashboard</a></li>
            <li><a routerLink="/profile/dashboard" (click)="toggleMobileMenu()">Profile Dashboard</a></li>
            <li><a routerLink="/profile/my-bookings" (click)="toggleMobileMenu()">My Bookings</a></li>
            <li><a routerLink="/profile/my-refunds" (click)="toggleMobileMenu()">My Refunds</a></li>
            <li><a (click)="logout(); toggleMobileMenu()" class="text-red-400">Logout</a></li>
          </ng-template>
        </ul>
      </div>
    </nav>

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
export class NavbarComponent implements OnInit {
  isTransparent = true;
  isHome = true;
  isMobileMenuOpen = false;
  currentUser$!: Observable<UserModel | null>;
  currentUser: UserModel | null = null;
  isAdmin = false;
  isAgent = false;
  isManager = false;
  isSupervisor = false;
  isDriver = false;
  canAccessAdminPanel = false;
  globalLogoUrl: string | null = null;
  expandedPictureUrl: string | null = null;
  unreadNotificationCount = 0;
  recentNotifications: NotificationItem[] = [];

  themes = [
    { id: 'light', name: 'light mode', desc: 'Clean & professional', primary: '#004f98', accent: '#1E87E4', surface: '#ffffff' },
    { id: 'dark', name: 'dark mode', desc: 'Sleek & eye-friendly', primary: '#3b82f6', accent: '#93c5fd', surface: '#0f172a' },
    { id: 'gray', name: 'gray mode', desc: 'Muted slate & silver', primary: '#475569', accent: '#94a3b8', surface: '#f1f5f9' },
    { id: 'blue', name: 'blue mode', desc: 'Vibrant ocean breeze', primary: '#0ea5e9', accent: '#7dd3fc', surface: '#f0f9ff' },
    { id: 'emerald', name: 'emerald mode', desc: 'Fresh & organic mint', primary: '#10b981', accent: '#6ee7b7', surface: '#f0fdf4' },
    { id: 'sunset', name: 'sunset mode', desc: 'Warm autumn glow', primary: '#f97316', accent: '#fdba74', surface: '#fff7ed' }
  ];
  activeTheme = 'light';

  constructor(
    private authService: AuthService, 
    private router: Router,
    private settingsService: SettingsService,
    private notificationService: NotificationService
  ) {
    this.router.events.subscribe(() => {
      this.isHome = this.router.url === '/' || this.router.url === '/home';
      this.checkScroll();
    });
  }

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('admin_theme');
    if (savedTheme && this.themes.find(t => t.id === savedTheme)) {
      this.activeTheme = savedTheme;
    }
    this.applyTheme();

    // Listen for theme changes from other components (like admin panel)
    window.addEventListener('admin-theme-changed', ((e: CustomEvent) => {
      if (e.detail && e.detail.theme) {
        this.activeTheme = e.detail.theme;
      }
    }) as EventListener);

    this.currentUser$ = this.authService.currentUser$;
    this.currentUser$.subscribe(user => {
      this.currentUser = user;
      
      if (this.currentUser && !this.currentUser.roleName) {
        this.currentUser.roleName = this.authService.getRoleName();
      }

      this.isAdmin = this.authService.isAdmin();
      this.isAgent = this.authService.isAgent();
      this.isManager = this.authService.isCompanyManager();
      this.isSupervisor = this.authService.isSupervisor();
      this.isDriver = this.authService.isDriver();
      this.canAccessAdminPanel = this.authService.canAccessAdminPanel();
      
      if (this.currentUser) {
        this.fetchNotifications();
      }
    });

    this.notificationService.unreadCount$.subscribe(cnt => {
      this.unreadNotificationCount = cnt;
    });

    this.notificationService.newNotification$.subscribe((notification: NotificationItem) => {
      this.recentNotifications.unshift(notification);
      if (this.recentNotifications.length > 5) {
        this.recentNotifications.pop();
      }
      this.playNotificationSound();
    });

    this.settingsService.getLogo().subscribe((res: any) => {
      if (res.success && res.data) {
        this.globalLogoUrl = res.data;
      }
    });
  }

  setTheme(themeId: string) {
    this.activeTheme = themeId;
    localStorage.setItem('admin_theme', themeId);
    this.applyTheme();
    window.dispatchEvent(new CustomEvent('admin-theme-changed', { detail: { theme: themeId } }));
  }

  applyTheme() {
    document.body.setAttribute('data-theme', this.activeTheme);
    document.documentElement.setAttribute('data-theme', this.activeTheme);
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.checkScroll();
  }

  private checkScroll() {
    const scrollPosition = window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0;
    this.isTransparent = scrollPosition < 50;
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  logout() {
    this.authService.logout();
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

  getInitials(name?: string): string {
    if (!name) return 'U';
    const parts = name.split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  fetchNotifications() {
    this.notificationService.getAll().subscribe((res: any) => {
      if (res.success && res.data) {
        this.recentNotifications = res.data.slice(0, 5);
      }
    });
  }

  markAsRead(id: number) {
    this.notificationService.markAsRead(id).subscribe(() => {
      this.fetchNotifications();
    });
  }

  markAllAsRead() {
    if (this.unreadNotificationCount > 0) {
      this.notificationService.markAllAsRead().subscribe(() => {
        this.fetchNotifications();
      });
    }
  }

  playNotificationSound() {
    // Play a standard pop/notification sound
    const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
    audio.volume = 0.5;
    audio.play().catch(e => console.log('Audio autoplay prevented by browser', e));
  }
}
