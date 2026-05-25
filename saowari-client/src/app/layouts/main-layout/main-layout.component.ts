import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';
import { SupportChatComponent } from '../../shared/components/support-chat/support-chat.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent, FooterComponent, SupportChatComponent],
  template: `
    <div class="flex flex-col min-h-screen">
      <app-navbar></app-navbar>
      
      <!-- Main Content Area with fade transition placeholder -->
      <main class="flex-grow">
        <router-outlet></router-outlet>
      </main>

      <app-footer></app-footer>
      
      <!-- Globally available floating support widget -->
      <app-support-chat></app-support-chat>
    </div>
  `
})
export class MainLayoutComponent {}
