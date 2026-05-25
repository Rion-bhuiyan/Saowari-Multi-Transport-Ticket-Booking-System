import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  email = '';
  isLoading = false;
  emailSent = false;

  constructor(
    private authService: AuthService,
    private notification: NotificationService,
    private router: Router
  ) {}

  onSubmit() {
    if (!this.email) {
      this.notification.warning('Please enter your email address.');
      return;
    }
    this.isLoading = true;
    // Simulate the forgot password flow (API call)
    setTimeout(() => {
      this.isLoading = false;
      this.emailSent = true;
      this.notification.success('If your email is registered, you will receive reset instructions.', 'Email Sent');
    }, 1500);
  }
}
