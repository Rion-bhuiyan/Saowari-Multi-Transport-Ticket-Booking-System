import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="loadingService.isLoading$ | async" 
         class="fixed inset-0 z-[9999] flex items-center justify-center bg-white/80 backdrop-blur-sm transition-opacity duration-300">
      <div class="flex flex-col items-center gap-4">
        <!-- Using a CSS spinner resembling a steering wheel or modern loader -->
        <span class="loading loading-spinner text-saowari-primary loading-lg scale-150"></span>
        <h3 class="text-saowari-primary-dark font-heading font-semibold text-xl tracking-wide animate-pulse">SAOWARI</h3>
        <p class="text-saowari-text-secondary text-sm">Loading your journey...</p>
      </div>
    </div>
  `
})
export class LoadingSpinnerComponent {
  constructor(public loadingService: LoadingService) {}
}
