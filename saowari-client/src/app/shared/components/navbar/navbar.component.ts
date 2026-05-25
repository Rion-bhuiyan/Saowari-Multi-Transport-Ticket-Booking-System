import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SettingsService } from '../../../core/services/api/settings.service';
import { Observable } from 'rxjs';
import { UserModel } from '../../../core/models/auth.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="bg-saowari-primary shadow-lg fixed w-full z-50 transition-all duration-300">
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

            <!-- Auth/Profile Section -->
            <ng-container *ngIf="currentUser === null; else loggedInMenu">
              <a routerLink="/auth/login" class="text-white hover:text-saowari-accent font-medium transition-colors">Login</a>
              <a routerLink="/auth/register" class="btn btn-outline border-white text-white hover:bg-white hover:text-saowari-primary rounded-full px-6">Sign Up</a>
            </ng-container>
            
            <ng-template #loggedInMenu>
              <div class="dropdown dropdown-end">
                <label tabindex="0" class="btn btn-ghost btn-circle avatar border-2 border-white/30 hover:border-white cursor-pointer">
                  <div class="w-10 rounded-full bg-saowari-primary-dark flex items-center justify-center text-white overflow-hidden animate-fade-in">
                    <span *ngIf="!currentUser?.picture" class="text-lg font-bold">{{ getInitials(currentUser?.fullName) }}</span>
                    <img *ngIf="currentUser?.picture" [src]="getProfilePictureUrl(currentUser?.picture)" alt="Avatar" class="w-full h-full object-cover hover:scale-105 transition-transform" />
                  </div>
                </label>
                <ul tabindex="0" class="mt-3 z-[1] p-2 shadow menu menu-sm dropdown-content bg-base-100 rounded-box w-52 text-saowari-text-primary">
                  <li class="menu-title px-4 py-2 border-b border-gray-100">
                    <span class="block font-semibold text-gray-800">{{ currentUser?.fullName }}</span>
                    <span class="block text-xs text-gray-500">{{ currentUser?.roleName }}</span>
                  </li>
                  <li *ngIf="isAdmin || isAgent"><a routerLink="/admin/dashboard" class="py-3"><i class="fas fa-shield-alt mr-2 text-saowari-primary"></i> Admin Panel</a></li>
                  <li><a routerLink="/profile/dashboard" class="py-3"><i class="fas fa-user mr-2 text-saowari-primary"></i> Profile Dashboard</a></li>
                  <li><a routerLink="/profile/my-bookings" class="py-3"><i class="fas fa-ticket-alt mr-2 text-saowari-primary"></i> My Bookings</a></li>
                  <div class="divider my-0"></div>
                  <li><a (click)="logout()" class="py-3 text-saowari-danger"><i class="fas fa-sign-out-alt mr-2"></i> Logout</a></li>
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
          <div class="divider bg-white/20 h-px my-2"></div>
          
          <ng-container *ngIf="currentUser === null; else mobileLoggedInMenu">
            <li><a routerLink="/auth/login" (click)="toggleMobileMenu()">Login</a></li>
            <li><a routerLink="/auth/register" (click)="toggleMobileMenu()" class="text-saowari-accent">Sign Up</a></li>
          </ng-container>
          
          <ng-template #mobileLoggedInMenu>
            <li class="menu-title text-gray-300">Welcome, {{ currentUser?.fullName }}</li>
            <li *ngIf="isAdmin || isAgent"><a routerLink="/admin/dashboard" (click)="toggleMobileMenu()">Admin Panel</a></li>
            <li><a routerLink="/profile/dashboard" (click)="toggleMobileMenu()">Profile Dashboard</a></li>
            <li><a routerLink="/profile/my-bookings" (click)="toggleMobileMenu()">My Bookings</a></li>
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
  globalLogoUrl: string | null = null;
  expandedPictureUrl: string | null = null;

  constructor(
    private authService: AuthService, 
    private router: Router,
    private settingsService: SettingsService
  ) {
    this.router.events.subscribe(() => {
      this.isHome = this.router.url === '/' || this.router.url === '/home';
      this.checkScroll();
    });
  }

  ngOnInit(): void {
    this.currentUser$ = this.authService.currentUser$;
    this.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.isAdmin = this.authService.isAdmin();
      this.isAgent = this.authService.isAgent();
    });

    this.settingsService.getLogo().subscribe((res: any) => {
      if (res.success && res.data) {
        this.globalLogoUrl = res.data;
      }
    });
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
}
