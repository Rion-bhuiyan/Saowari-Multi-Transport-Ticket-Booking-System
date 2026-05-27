import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RegisterDto } from '../../../core/models/auth.model';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  model: RegisterDto = {
    fullName: '',
    email: '',
    phone: '',
    password: '',
    roleId: 3 // Default: Customer
  };
  confirmPassword = '';
  showPassword = false;
  showConfirmPassword = false;
  isLoading = false;
  isOtpRequired = false;
  otpCode = '';
  returnUrl: string = '';

  constructor(
    private authService: AuthService,
    private notification: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.route.queryParams.subscribe(params => {
      this.returnUrl = params['returnUrl'] || '';
    });
  }

  get passwordsMatch(): boolean {
    return this.model.password === this.confirmPassword;
  }

  onRegister() {
    if (!this.passwordsMatch) {
      this.notification.error('Passwords do not match.', 'Validation Error');
      return;
    }
    this.isLoading = true;
    this.model.confirmPassword = this.confirmPassword;
    this.authService.register(this.model).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.notification.success('Account created! Please log in.', 'Registration Successful');
          if (this.returnUrl) {
            this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.returnUrl } });
          } else {
            this.router.navigate(['/auth/login']);
          }
        } else if (res.message === 'OTP_REQUIRED') {
          this.notification.success('Account created! Please check your email for the verification code.', 'Verification Required');
          this.isOtpRequired = true;
        } else {
          this.notification.error(res.message || 'Registration failed.', 'Error');
        }
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('An error occurred. Please try again.', 'Error');
      }
    });
  }

  onVerifyOtp() {
    if (!this.otpCode || this.otpCode.length < 6) {
      this.notification.error('Please enter a valid 6-digit OTP code.');
      return;
    }
    
    this.isLoading = true;
    this.authService.verifyRegistrationOtp(this.model.email, this.otpCode).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.notification.success('Email verified successfully! Logging you in...', 'Success');
          if (this.returnUrl) {
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.router.navigate(['/home']);
          }
        } else {
          this.notification.error(res.message || 'Verification failed.', 'Error');
        }
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('An error occurred. Please try again.', 'Error');
      }
    });
  }
}
