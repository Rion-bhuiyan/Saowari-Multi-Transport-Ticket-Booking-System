import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoginDto } from '../../../core/models/auth.model';

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

  constructor(
    private authService: AuthService,
    private notification: NotificationService,
    private router: Router
  ) {}

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
          this.notification.success('Welcome back!', 'Login Successful');
          const role = res.data?.user?.roleName || res.data?.user?.RoleName;
          if (role === 'Admin' || role === 'Agent') {
            this.router.navigate(['/admin/dashboard']);
          } else if (role === 'CompanyManager') {
            this.router.navigate(['/admin/manager-dashboard']);
          } else if (role === 'Supervisor') {
            this.router.navigate(['/admin/supervisor-dashboard']);
          } else {
            this.router.navigate(['/home']);
          }
        } else {
          this.notification.error(res.message || 'Invalid credentials.', 'Login Failed');
        }
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('An error occurred. Please try again.', 'Error');
      }
    });
  }
}
