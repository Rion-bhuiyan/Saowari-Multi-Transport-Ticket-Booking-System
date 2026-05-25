import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen bg-saowari-surface-alt flex items-center justify-center p-4">
      <div class="w-full max-w-5xl">
        <router-outlet></router-outlet>
      </div>
    </div>
  `
})
export class AuthLayoutComponent {}
