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
         class="fixed inset-0 z-[9999] flex items-center justify-center bg-slate-950/80 backdrop-blur-md transition-opacity duration-500">
      
      <!-- Glowing Background Aura based on Active State -->
      <div class="absolute inset-0 pointer-events-none overflow-hidden flex items-center justify-center">
        <div class="w-[30rem] h-[30rem] rounded-full blur-[120px] transition-colors duration-1000 ease-in-out"
             [ngClass]="{
               'bg-emerald-600/30': activeIndex === 0,
               'bg-blue-600/30': activeIndex === 1,
               'bg-cyan-500/30': activeIndex === 2
             }">
        </div>
      </div>

      <!-- Main Glassmorphic Card -->
      <div class="relative w-full max-w-sm mx-4 bg-slate-900/60 border border-slate-700/50 rounded-3xl p-8 shadow-2xl backdrop-blur-xl flex flex-col items-center overflow-hidden">
        
        <!-- Animated Ring Glow -->
        <div class="absolute -inset-1 rounded-3xl opacity-50 blur-md transition-colors duration-1000 ease-in-out animate-pulse"
             [ngClass]="{
               'bg-emerald-500/40': activeIndex === 0,
               'bg-blue-500/40': activeIndex === 1,
               'bg-cyan-400/40': activeIndex === 2
             }">
        </div>

        <div class="relative z-10 w-full flex flex-col items-center">
          
          <!-- Animation Container -->
          <div class="relative w-48 h-48 mb-6 flex items-center justify-center">
            
            <!-- 1. Bus Animation -->
            <div class="absolute inset-0 transition-all duration-700 ease-in-out transform flex flex-col items-center justify-center"
                 [ngClass]="activeIndex === 0 ? 'opacity-100 scale-100' : 'opacity-0 scale-90 pointer-events-none'">
              <div class="relative w-32 h-32 text-emerald-400 animate-bounce-subtle">
                <!-- Bus SVG -->
                <svg viewBox="0 0 100 100" fill="currentColor" class="w-full h-full drop-shadow-lg">
                  <path d="M15 35 C15 25, 25 20, 50 20 C75 20, 85 25, 85 35 L85 70 C85 75, 80 80, 75 80 L25 80 C20 80, 15 75, 15 70 Z" fill="#1e293b" stroke="currentColor" stroke-width="4"/>
                  <!-- Windshield -->
                  <path d="M20 35 L80 35 L80 50 L20 50 Z" fill="#38bdf8"/>
                  <!-- Grill & Lights -->
                  <rect x="35" y="60" width="30" height="10" rx="2" fill="#334155"/>
                  <circle cx="25" cy="65" r="4" fill="#fbbf24" class="animate-pulse-fast"/>
                  <circle cx="75" cy="65" r="4" fill="#fbbf24" class="animate-pulse-fast"/>
                  <!-- Mirrors -->
                  <path d="M10 40 L15 40 L15 50 L12 50 Z" fill="#475569"/>
                  <path d="M90 40 L85 40 L85 50 L88 50 Z" fill="#475569"/>
                </svg>
              </div>
              <!-- Moving Road Lines -->
              <div class="absolute bottom-0 w-48 h-1 overflow-hidden mt-2">
                <div class="w-[200%] h-full flex bg-slate-800 animate-scroll-fast">
                   <div class="w-1/4 h-full bg-emerald-500 mx-2"></div>
                   <div class="w-1/4 h-full bg-emerald-500 mx-2"></div>
                   <div class="w-1/4 h-full bg-emerald-500 mx-2"></div>
                   <div class="w-1/4 h-full bg-emerald-500 mx-2"></div>
                </div>
              </div>
            </div>

            <!-- 2. Launch Animation -->
            <div class="absolute inset-0 transition-all duration-700 ease-in-out transform flex flex-col items-center justify-center"
                 [ngClass]="activeIndex === 1 ? 'opacity-100 scale-100 translate-x-0' : 'opacity-0 scale-90 pointer-events-none translate-x-4'">
              <div class="relative w-36 h-32 text-blue-400 animate-rocking">
                <!-- Launch SVG -->
                <svg viewBox="0 0 100 100" fill="currentColor" class="w-full h-full drop-shadow-lg">
                  <!-- Hull -->
                  <path d="M10 70 L90 70 L80 85 C60 90, 40 90, 20 85 Z" fill="#1e293b" stroke="currentColor" stroke-width="4"/>
                  <!-- Lower Deck -->
                  <rect x="20" y="55" width="60" height="15" fill="#334155"/>
                  <circle cx="30" cy="62" r="3" fill="#60a5fa" class="animate-pulse"/>
                  <circle cx="50" cy="62" r="3" fill="#60a5fa" class="animate-pulse"/>
                  <circle cx="70" cy="62" r="3" fill="#60a5fa" class="animate-pulse"/>
                  <!-- Upper Deck -->
                  <rect x="30" y="40" width="40" height="15" fill="#475569"/>
                  <circle cx="40" cy="47" r="2.5" fill="#60a5fa"/>
                  <circle cx="60" cy="47" r="2.5" fill="#60a5fa"/>
                  <!-- Chimney -->
                  <rect x="45" y="25" width="10" height="15" fill="#94a3b8"/>
                  <path d="M42 20 Q50 10 58 20" fill="none" stroke="#cbd5e1" stroke-width="3" class="animate-smoke"/>
                </svg>
              </div>
              <!-- Moving Water Waves -->
              <div class="absolute bottom-2 w-48 h-4 overflow-hidden flex items-end opacity-70">
                 <svg viewBox="0 0 200 20" class="w-[200%] h-full fill-blue-500 animate-scroll-waves">
                   <path d="M0 10 Q25 0 50 10 T100 10 T150 10 T200 10 L200 20 L0 20 Z"/>
                 </svg>
              </div>
            </div>

            <!-- 3. Plane Animation -->
            <div class="absolute inset-0 transition-all duration-700 ease-in-out transform flex flex-col items-center justify-center"
                 [ngClass]="activeIndex === 2 ? 'opacity-100 scale-100 translate-y-0' : 'opacity-0 scale-90 pointer-events-none translate-y-4'">
              <div class="relative w-40 h-40 text-cyan-400 animate-floating">
                <!-- Plane SVG -->
                <svg viewBox="0 0 100 100" fill="currentColor" class="w-full h-full drop-shadow-xl transform -rotate-12">
                  <!-- Body -->
                  <path d="M20 50 C20 40, 80 40, 90 50 C80 60, 20 60, 20 50 Z" fill="#e2e8f0"/>
                  <!-- Wings -->
                  <path d="M40 50 L30 20 L45 20 L55 50 Z" fill="#94a3b8"/>
                  <path d="M40 50 L30 80 L45 80 L55 50 Z" fill="#cbd5e1"/>
                  <!-- Tail -->
                  <path d="M25 50 L15 35 L25 35 L30 50 Z" fill="#94a3b8"/>
                  <!-- Cockpit -->
                  <path d="M80 47 Q85 47 85 50 L80 50 Z" fill="#38bdf8"/>
                  <!-- Engine Trails -->
                  <line x1="5" y1="50" x2="15" y2="50" stroke="#bae6fd" stroke-width="2" class="animate-pulse-fast"/>
                  <line x1="0" y1="45" x2="10" y2="45" stroke="#bae6fd" stroke-width="1" class="animate-pulse-fast"/>
                  <line x1="0" y1="55" x2="10" y2="55" stroke="#bae6fd" stroke-width="1" class="animate-pulse-fast"/>
                </svg>
              </div>
              <!-- Moving Clouds -->
              <div class="absolute bottom-6 w-56 h-12 overflow-hidden flex items-center justify-between opacity-40 animate-scroll-clouds">
                 <div class="w-10 h-3 bg-white rounded-full blur-[2px]"></div>
                 <div class="w-16 h-4 bg-white rounded-full blur-[2px] mt-4"></div>
                 <div class="w-8 h-2 bg-white rounded-full blur-[1px] mb-2"></div>
              </div>
            </div>

          </div>

          <!-- Title -->
          <h3 class="text-white font-heading font-bold text-2xl tracking-[0.2em] mb-2 animate-pulse drop-shadow-md">SAOWARI</h3>
          
          <!-- Dynamic Status Text -->
          <p class="text-slate-300 text-sm font-medium h-6 transition-all duration-300 ease-in-out text-center">
            {{ statusTexts[activeIndex] }}
          </p>

          <!-- Loading Progress Bar -->
          <div class="w-full h-1 bg-slate-800 rounded-full mt-6 overflow-hidden">
             <div class="h-full bg-gradient-to-r from-emerald-400 via-blue-500 to-cyan-400 animate-progress-bar rounded-full"></div>
          </div>
          
        </div>
      </div>
    </div>
  `,
  styles: [`
    .animate-bounce-subtle {
      animation: bounceSubtle 0.5s infinite alternate ease-in-out;
    }
    @keyframes bounceSubtle {
      0% { transform: translateY(0px); }
      100% { transform: translateY(-4px); }
    }

    .animate-scroll-fast {
      animation: scrollFast 0.6s linear infinite;
    }
    @keyframes scrollFast {
      0% { transform: translateX(0); }
      100% { transform: translateX(-50%); }
    }

    .animate-pulse-fast {
      animation: pulseFast 1s cubic-bezier(0.4, 0, 0.6, 1) infinite;
    }
    @keyframes pulseFast {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.4; }
    }

    .animate-rocking {
      animation: rocking 3s infinite ease-in-out;
      transform-origin: bottom center;
    }
    @keyframes rocking {
      0%, 100% { transform: rotate(-3deg) translateY(0); }
      50% { transform: rotate(3deg) translateY(-5px); }
    }

    .animate-scroll-waves {
      animation: scrollWaves 2s linear infinite;
    }
    @keyframes scrollWaves {
      0% { transform: translateX(0); }
      100% { transform: translateX(-50%); }
    }

    .animate-smoke {
      stroke-dasharray: 10;
      animation: smokeRise 2s linear infinite;
    }
    @keyframes smokeRise {
      0% { stroke-dashoffset: 20; opacity: 1; }
      100% { stroke-dashoffset: 0; opacity: 0; }
    }

    .animate-floating {
      animation: floating 3s ease-in-out infinite;
    }
    @keyframes floating {
      0%, 100% { transform: translateY(0) translateX(0); }
      50% { transform: translateY(-10px) translateX(5px); }
    }

    .animate-scroll-clouds {
      animation: scrollClouds 4s linear infinite;
    }
    @keyframes scrollClouds {
      0% { transform: translateX(100%); }
      100% { transform: translateX(-100%); }
    }

    .animate-progress-bar {
      width: 100%;
      animation: progress 2.5s ease-in-out infinite;
      transform-origin: left;
    }
    @keyframes progress {
      0% { transform: scaleX(0); opacity: 0.5; }
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

