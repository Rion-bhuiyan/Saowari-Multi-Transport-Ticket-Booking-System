import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoginDto } from '../../../core/models/auth.model';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  credentials: LoginDto = { email: '', password: '' };
  showPassword = false;
  isLoading = false;

  isOtpRequired = false;
  otpCode = '';
  otpType: 'login' | 'registration' = 'login';

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

  onLogin() {
    if (!this.credentials.email || !this.credentials.password) {
      this.notification.warning('Please enter your email and password.');
      return;
    }
    this.isLoading = true;
    this.authService.login(this.credentials).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.handleSuccessfulLogin(res);
        } else {
          if (res.message === 'NEW_DEVICE_OTP_REQUIRED') {
            this.isOtpRequired = true;
            this.otpType = 'login';
            this.notification.warning('Please check your email for the OTP code.', 'New Device Detected');
          } else if (res.message === 'UNVERIFIED_EMAIL_OTP_SENT') {
            this.isOtpRequired = true;
            this.otpType = 'registration';
            this.notification.warning('You must verify your email before logging in. A new code was sent to your email.', 'Email Verification Required');
          } else {
            this.notification.error(res.message || 'Invalid credentials.', 'Login Failed');
          }
        }
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('An error occurred. Please try again.', 'Error');
      }
    });
  }

  onVerifyOtp() {
    if (!this.otpCode) {
      this.notification.warning('Please enter the OTP code sent to your email.');
      return;
    }
    
    this.isLoading = true;

    const request$ = this.otpType === 'login' 
      ? this.authService.verifyLoginOtp(this.credentials.email, this.otpCode)
      : this.authService.verifyRegistrationOtp(this.credentials.email, this.otpCode);

    request$.subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res.success) {
          this.handleSuccessfulLogin(res);
        } else {
          this.notification.error(res.message || 'Invalid or expired OTP.', 'Verification Failed');
        }
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('An error occurred during verification.', 'Error');
      }
    });
  }

  private handleSuccessfulLogin(res: any) {
    this.notification.success('Welcome back!', 'Login Successful');
    const role = res.data?.user?.roleName || res.data?.user?.RoleName;
    if (role === 'Admin' || role === 'Agent') {
      this.router.navigate(['/admin/dashboard']);
    } else if (role === 'CompanyManager') {
      this.router.navigate(['/admin/manager-dashboard']);
    } else if (role === 'Supervisor') {
      this.router.navigate(['/admin/supervisor-dashboard']);
    } else if (role === 'Driver') {
      this.router.navigate(['/admin/driver-dashboard']);
    } else {
      if (this.returnUrl) {
        this.router.navigateByUrl(this.returnUrl);
      } else {
        this.router.navigate(['/home']);
      }
    }
  }
}
