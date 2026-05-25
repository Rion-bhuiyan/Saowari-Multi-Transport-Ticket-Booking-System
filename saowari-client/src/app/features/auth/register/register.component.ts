import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RegisterDto } from '../../../core/models/auth.model';

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

  constructor(
    private authService: AuthService,
    private notification: NotificationService,
    private router: Router
  ) {}

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
          this.router.navigate(['/auth/login']);
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
}
