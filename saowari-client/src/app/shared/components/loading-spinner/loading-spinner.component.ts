import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="isLoading" 
         class="fixed inset-0 z-[9999] flex items-center justify-center bg-saowari-surface-alt/95 backdrop-blur-2xl transition-opacity duration-500">
      
      <!-- Glowing Background Aura based on Active State -->
      <div class="absolute inset-0 pointer-events-none overflow-hidden flex items-center justify-center opacity-70">
        <div class="absolute w-[60vw] h-[60vw] max-w-[40rem] max-h-[40rem] rounded-full blur-[120px] transition-all duration-1000 ease-in-out mix-blend-screen"
             [ngClass]="{
               'bg-saowari-primary/50 scale-110 translate-x-10': activeIndex === 0,
               'bg-saowari-secondary/50 scale-95 -translate-x-10': activeIndex === 1,
               'bg-saowari-accent/50 scale-105 translate-y-10': activeIndex === 2
             }">
        </div>
        <div class="absolute w-[50vw] h-[50vw] max-w-[30rem] max-h-[30rem] rounded-full blur-[100px] transition-all duration-1000 ease-in-out mix-blend-multiply opacity-50"
             [ngClass]="{
               'bg-saowari-secondary/40 scale-90 -translate-y-10': activeIndex === 0,
               'bg-saowari-accent/40 scale-110 translate-x-10': activeIndex === 1,
               'bg-saowari-primary/40 scale-100 -translate-x-10': activeIndex === 2
             }">
        </div>
      </div>

      <!-- Main Full-Screen Content Wrapper -->
      <div class="relative z-10 w-full h-full flex flex-col items-center justify-center px-6">
        
        <div class="relative w-full max-w-lg mx-auto flex flex-col items-center">
          
          <!-- Animation Container (Responsive sizing) -->
          <div class="relative w-48 h-48 sm:w-64 sm:h-64 mb-8 flex items-center justify-center">
            
            <!-- 1. Bus Animation -->
            <div class="absolute inset-0 transition-all duration-700 ease-in-out transform flex flex-col items-center justify-center"
                 [ngClass]="activeIndex === 0 ? 'opacity-100 scale-100' : 'opacity-0 scale-90 pointer-events-none'">
              <div class="relative w-32 h-32 sm:w-40 sm:h-40 animate-bounce-subtle text-saowari-text-primary">
                <!-- Bus SVG -->
                <svg viewBox="0 0 100 100" fill="currentColor" class="w-full h-full drop-shadow-2xl">
                  <path d="M15 35 C15 25, 25 20, 50 20 C75 20, 85 25, 85 35 L85 70 C85 75, 80 80, 75 80 L25 80 C20 80, 15 75, 15 70 Z" fill="currentColor"/>
                  <!-- Windshield -->
                  <path d="M20 35 L80 35 L80 50 L20 50 Z" class="text-saowari-primary-light" fill="currentColor"/>
                  <path d="M25 35 L75 35 L75 48 L25 48 Z" class="text-saowari-primary opacity-60" fill="currentColor"/>
                  <!-- Grill & Lights -->
                  <rect x="35" y="60" width="30" height="10" rx="2" class="text-saowari-surface-alt" fill="currentColor"/>
                  <circle cx="25" cy="65" r="4" class="text-saowari-secondary animate-pulse-fast" fill="currentColor"/>
                  <circle cx="75" cy="65" r="4" class="text-saowari-secondary animate-pulse-fast" fill="currentColor"/>
                  <!-- Mirrors -->
                  <path d="M10 40 L15 40 L15 50 L12 50 Z" class="text-saowari-text-secondary" fill="currentColor"/>
                  <path d="M90 40 L85 40 L85 50 L88 50 Z" class="text-saowari-text-secondary" fill="currentColor"/>
                  <!-- Logo/Badge -->
                  <circle cx="50" cy="55" r="3" class="text-saowari-accent" fill="currentColor"/>
                </svg>
              </div>
              <!-- Moving Road Lines -->
              <div class="absolute bottom-4 w-56 sm:w-72 h-1.5 overflow-hidden rounded-full">
                <div class="w-[200%] h-full flex bg-saowari-surface-alt animate-scroll-fast shadow-inner">
                   <div class="w-1/4 h-full bg-saowari-primary mx-2 rounded-full"></div>
                   <div class="w-1/4 h-full bg-saowari-primary mx-2 rounded-full"></div>
                   <div class="w-1/4 h-full bg-saowari-primary mx-2 rounded-full"></div>
                   <div class="w-1/4 h-full bg-saowari-primary mx-2 rounded-full"></div>
                </div>
              </div>
            </div>

            <!-- 2. Launch Animation -->
            <div class="absolute inset-0 transition-all duration-700 ease-in-out transform flex flex-col items-center justify-center"
                 [ngClass]="activeIndex === 1 ? 'opacity-100 scale-100 translate-x-0' : 'opacity-0 scale-90 pointer-events-none translate-x-8'">
              <div class="relative w-36 h-36 sm:w-48 sm:h-48 text-saowari-text-primary animate-rocking">
                <!-- Launch SVG -->
                <svg viewBox="0 0 100 100" fill="currentColor" class="w-full h-full drop-shadow-2xl">
                  <!-- Hull -->
                  <path d="M10 70 L90 70 L80 85 C60 90, 40 90, 20 85 Z" fill="currentColor"/>
                  <!-- Lower Deck -->
                  <rect x="20" y="55" width="60" height="15" class="text-saowari-text-secondary" fill="currentColor"/>
                  <circle cx="30" cy="62" r="3" class="text-saowari-primary-light animate-pulse" fill="currentColor"/>
                  <circle cx="50" cy="62" r="3" class="text-saowari-primary-light animate-pulse" fill="currentColor"/>
                  <circle cx="70" cy="62" r="3" class="text-saowari-primary-light animate-pulse" fill="currentColor"/>
                  <!-- Upper Deck -->
                  <rect x="30" y="40" width="40" height="15" class="text-saowari-primary" fill="currentColor"/>
                  <circle cx="40" cy="47" r="2.5" class="text-white" fill="currentColor"/>
                  <circle cx="60" cy="47" r="2.5" class="text-white" fill="currentColor"/>
                  <!-- Chimney -->
                  <rect x="45" y="25" width="10" height="15" class="text-saowari-surface-alt" fill="currentColor"/>
                  <path d="M42 20 Q50 10 58 20" fill="none" class="text-saowari-border animate-smoke" stroke="currentColor" stroke-width="4" stroke-linecap="round"/>
                </svg>
              </div>
              <!-- Moving Water Waves -->
              <div class="absolute bottom-6 w-56 sm:w-72 h-6 overflow-hidden flex items-end opacity-80">
                 <svg viewBox="0 0 200 20" class="w-[200%] h-full text-saowari-primary fill-current animate-scroll-waves">
                   <path d="M0 10 Q25 0 50 10 T100 10 T150 10 T200 10 L200 20 L0 20 Z"/>
                 </svg>
              </div>
            </div>

            <!-- 3. Plane Animation -->
            <div class="absolute inset-0 transition-all duration-700 ease-in-out transform flex flex-col items-center justify-center"
                 [ngClass]="activeIndex === 2 ? 'opacity-100 scale-100 translate-y-0' : 'opacity-0 scale-90 pointer-events-none translate-y-8'">
              <div class="relative w-40 h-40 sm:w-56 sm:h-56 animate-floating text-saowari-text-primary">
                <!-- Plane SVG -->
                <svg viewBox="0 0 100 100" fill="currentColor" class="w-full h-full drop-shadow-2xl transform -rotate-12">
                  <!-- Body -->
                  <path d="M20 50 C20 40, 80 40, 90 50 C80 60, 20 60, 20 50 Z" class="text-saowari-text-primary" fill="currentColor"/>
                  <!-- Wings -->
                  <path d="M40 50 L30 20 L45 20 L55 50 Z" class="text-saowari-text-secondary" fill="currentColor"/>
                  <path d="M40 50 L30 80 L45 80 L55 50 Z" class="text-saowari-text-secondary opacity-80" fill="currentColor"/>
                  <!-- Tail -->
                  <path d="M25 50 L15 35 L25 35 L30 50 Z" class="text-saowari-text-secondary" fill="currentColor"/>
                  <!-- Cockpit -->
                  <path d="M80 47 Q85 47 85 50 L80 50 Z" class="text-saowari-primary" fill="currentColor"/>
                  <!-- Engine Trails -->
                  <line x1="5" y1="50" x2="-10" y2="50" class="text-saowari-primary animate-pulse-fast" stroke="currentColor" stroke-width="3" stroke-linecap="round"/>
                  <line x1="0" y1="42" x2="-15" y2="42" class="text-saowari-accent animate-pulse-fast" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                  <line x1="0" y1="58" x2="-15" y2="58" class="text-saowari-accent animate-pulse-fast" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
                </svg>
              </div>
              <!-- Moving Clouds -->
              <div class="absolute bottom-10 w-64 sm:w-80 h-16 overflow-hidden flex items-center justify-between opacity-50 animate-scroll-clouds">
                 <div class="w-12 h-3 bg-saowari-border rounded-full blur-[2px]"></div>
                 <div class="w-20 h-5 bg-saowari-border rounded-full blur-[3px] mt-6"></div>
                 <div class="w-10 h-2 bg-saowari-border rounded-full blur-[1px] mb-4"></div>
              </div>
            </div>

          </div>

          <!-- Title & Brand -->
          <div class="flex flex-col items-center justify-center text-center mt-4">
            <h3 class="text-saowari-text-primary font-heading font-black text-3xl sm:text-4xl md:text-5xl tracking-[0.25em] mb-4 animate-pulse drop-shadow-lg">
              SAOWARI
            </h3>
            
            <!-- Dynamic Status Text -->
            <p class="text-saowari-text-secondary text-base sm:text-lg font-medium h-8 transition-all duration-300 ease-in-out">
              {{ statusTexts[activeIndex] }}
            </p>

            <!-- Loading Progress Bar -->
            <div class="w-64 sm:w-80 max-w-[80vw] h-1.5 bg-saowari-surface rounded-full mt-8 overflow-hidden shadow-inner border border-saowari-border/30">
               <div class="h-full bg-gradient-to-r from-saowari-primary via-saowari-secondary to-saowari-accent animate-progress-bar rounded-full"></div>
            </div>
          </div>
          
        </div>
      </div>
    </div>
  `,
  styles: [`
    .animate-bounce-subtle {
      animation: bounceSubtle 0.6s infinite alternate ease-in-out;
    }
    @keyframes bounceSubtle {
      0% { transform: translateY(0px) scale(1); }
      100% { transform: translateY(-8px) scale(1.02); }
    }

    .animate-scroll-fast {
      animation: scrollFast 0.8s linear infinite;
    }
    @keyframes scrollFast {
      0% { transform: translateX(0); }
      100% { transform: translateX(-50%); }
    }

    .animate-pulse-fast {
      animation: pulseFast 1.2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
    }
    @keyframes pulseFast {
      0%, 100% { opacity: 1; transform: scale(1); }
      50% { opacity: 0.3; transform: scale(0.9); }
    }

    .animate-rocking {
      animation: rocking 3.5s infinite ease-in-out;
      transform-origin: bottom center;
    }
    @keyframes rocking {
      0%, 100% { transform: rotate(-4deg) translateY(0); }
      50% { transform: rotate(4deg) translateY(-8px); }
    }

    .animate-scroll-waves {
      animation: scrollWaves 2.5s linear infinite;
    }
    @keyframes scrollWaves {
      0% { transform: translateX(0); }
      100% { transform: translateX(-50%); }
    }

    .animate-smoke {
      stroke-dasharray: 12;
      animation: smokeRise 2s linear infinite;
    }
    @keyframes smokeRise {
      0% { stroke-dashoffset: 24; opacity: 1; transform: translateY(0) scale(1); }
      100% { stroke-dashoffset: 0; opacity: 0; transform: translateY(-10px) scale(1.5); }
    }

    .animate-floating {
      animation: floating 3.5s ease-in-out infinite;
    }
    @keyframes floating {
      0%, 100% { transform: translateY(0) translateX(0); }
      50% { transform: translateY(-15px) translateX(8px); }
    }

    .animate-scroll-clouds {
      animation: scrollClouds 5s linear infinite;
    }
    @keyframes scrollClouds {
      0% { transform: translateX(100%); }
      100% { transform: translateX(-100%); }
    }

    .animate-progress-bar {
      width: 100%;
      animation: progress 2s ease-in-out infinite;
      transform-origin: left;
    }
    @keyframes progress {
      0% { transform: scaleX(0); opacity: 0.8; }
      50% { opacity: 1; }
      100% { transform: scaleX(1); opacity: 0; }
    }
  `]
})
export class LoadingSpinnerComponent implements OnInit, OnDestroy {
  isLoading = false;
  activeIndex = 0;
  
  statusTexts = [
    'Routing highways & preparing coaches...',
    'Navigating rivers & verifying launch docks...',
    'Clearing flight corridors & checking skies...'
  ];

  private sub?: Subscription;
  private intervalId?: any;

  constructor(private loadingService: LoadingService) {}

  ngOnInit(): void {
    this.sub = this.loadingService.isLoading$.subscribe(state => {
      this.isLoading = state;
      if (this.isLoading) {
        this.startCycle();
      } else {
        this.stopCycle();
      }
    });
  }

  ngOnDestroy(): void {
    this.stopCycle();
    if (this.sub) {
      this.sub.unsubscribe();
    }
  }

  private startCycle(): void {
    this.activeIndex = 0;
    this.stopCycle();
    this.intervalId = setInterval(() => {
      this.activeIndex = (this.activeIndex + 1) % 3;
    }, 2500);
  }

  private stopCycle(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = undefined;
    }
  }
}
