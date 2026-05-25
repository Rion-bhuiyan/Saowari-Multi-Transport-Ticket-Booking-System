import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SettingsService } from '../../../core/services/api/settings.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <footer class="bg-saowari-text-primary text-white pt-16 pb-8">
      <div class="container mx-auto px-4">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-12">
          
          <!-- Column 1 -->
          <div>
            <div class="flex items-center gap-2 mb-4 h-12 overflow-hidden">
              <img *ngIf="globalLogoUrl" [src]="globalLogoUrl" alt="SAOWARI" class="max-h-full max-w-[200px] object-contain" />
              <h2 *ngIf="!globalLogoUrl" class="text-2xl font-heading font-bold text-white">Saowari</h2>
            </div>
            <p class="text-gray-400 mb-6">Your Journey, Our Priority. Book bus, launch, and flight tickets instantly with seamless payments and 24/7 support.</p>
            <div class="flex gap-4">
              <a href="#" class="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center hover:bg-saowari-accent transition-colors"><i class="fab fa-facebook-f"></i></a>
              <a href="#" class="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center hover:bg-saowari-accent transition-colors"><i class="fab fa-twitter"></i></a>
              <a href="#" class="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center hover:bg-saowari-accent transition-colors"><i class="fab fa-instagram"></i></a>
            </div>
          </div>

          <!-- Column 2 -->
          <div>
            <h3 class="text-lg font-heading font-semibold text-white mb-4">Quick Links</h3>
            <ul class="space-y-2 text-gray-400">
              <li><a routerLink="/home" class="hover:text-saowari-accent transition-colors">Home</a></li>
              <li><a routerLink="/search" class="hover:text-saowari-accent transition-colors">Search Tickets</a></li>
              <li><a routerLink="/about" class="hover:text-saowari-accent transition-colors">About Us</a></li>
              <li><a routerLink="/contact" class="hover:text-saowari-accent transition-colors">Contact Support</a></li>
              <li><a routerLink="/faq" class="hover:text-saowari-accent transition-colors">FAQs</a></li>
            </ul>
          </div>

          <!-- Column 3 -->
          <div>
            <h3 class="text-lg font-heading font-semibold text-white mb-4">Transport Types</h3>
            <ul class="space-y-2 text-gray-400">
              <li><a routerLink="/search" [queryParams]="{transportType: 'Bus'}" class="hover:text-saowari-accent transition-colors">Intercity Buses</a></li>
              <li><a routerLink="/search" [queryParams]="{transportType: 'Launch'}" class="hover:text-saowari-accent transition-colors">River Launches</a></li>
              <li><a routerLink="/search" [queryParams]="{transportType: 'Airplane'}" class="hover:text-saowari-accent transition-colors">Domestic Flights</a></li>
            </ul>
          </div>

          <!-- Column 4 -->
          <div>
            <h3 class="text-lg font-heading font-semibold text-white mb-4">Contact Us</h3>
            <ul class="space-y-4 text-gray-400">
              <li class="flex items-start gap-3">
                <span class="text-saowari-accent mt-1"><svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg></span>
                <span>123 IT Avenue, Tech District<br>Dhaka, Bangladesh</span>
              </li>
              <li class="flex items-center gap-3">
                <span class="text-saowari-accent"><svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" /></svg></span>
                <span>+880 1234 567890</span>
              </li>
              <li class="flex items-center gap-3">
                <span class="text-saowari-accent"><svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg></span>
                <span>support&#64;saowari.com</span>
              </li>
            </ul>
          </div>
        </div>

        <div class="border-t border-gray-800 pt-8 mt-8 flex flex-col md:flex-row items-center justify-between">
          <p class="text-gray-500 text-sm mb-4 md:mb-0">&copy; {{ currentYear }} Saowari Booking System. All rights reserved.</p>
          <div class="flex gap-4 text-sm text-gray-500">
            <a routerLink="/legal" [queryParams]="{policy: 'privacy'}" class="hover:text-white transition-colors">Privacy Policy</a>
            <a routerLink="/legal" [queryParams]="{policy: 'terms'}" class="hover:text-white transition-colors">Terms of Service</a>
            <a routerLink="/legal" [queryParams]="{policy: 'cancellation'}" class="hover:text-white transition-colors">Cancellation Policy</a>
          </div>
        </div>
      </div>
    </footer>
  `
})
export class FooterComponent implements OnInit {
  currentYear = new Date().getFullYear();
  globalLogoUrl: string | null = null;

  constructor(private settingsService: SettingsService) {}

  ngOnInit(): void {
    this.settingsService.getLogo().subscribe((res: any) => {
      if (res.success && res.data) {
        this.globalLogoUrl = res.data;
      }
    });
  }
}
