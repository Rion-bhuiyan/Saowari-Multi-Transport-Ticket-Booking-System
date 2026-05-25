import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/api/user.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent implements OnInit {
  model = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  showCurrent = false;
  showNew = false;
  showConfirm = false;
  isSaving = false;
  userId: number | null = null;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      if (user) this.userId = user.userId;
    });
  }

  get passwordStrength(): number {
    const p = this.model.newPassword;
    let score = 0;
    if (p.length >= 8) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;
    return score;
  }

  get strengthLabel(): string {
    const labels = ['', 'Weak', 'Fair', 'Good', 'Strong'];
    return labels[this.passwordStrength] || '';
  }

  get strengthColor(): string {
    const colors = ['', 'progress-error', 'progress-warning', 'progress-info', 'progress-success'];
    return colors[this.passwordStrength] || '';
  }

  get passwordsMatch(): boolean {
    return !this.model.confirmPassword || this.model.newPassword === this.model.confirmPassword;
  }

  onSave() {
    if (!this.userId) return;
    if (this.model.newPassword !== this.model.confirmPassword) {
      this.notification.error('Passwords do not match.', 'Validation Error');
      return;
    }
    if (this.passwordStrength < 2) {
      this.notification.warning('Please choose a stronger password.');
      return;
    }
    this.isSaving = true;
    this.userService.changePassword(this.userId, this.model.currentPassword, this.model.newPassword).subscribe({
      next: (res: any) => {
        this.isSaving = false;
        if (res.success) {
          this.notification.success('Password changed successfully!', 'Success');
          this.model = { currentPassword: '', newPassword: '', confirmPassword: '' };
        } else {
          this.notification.error(res.message || 'Failed to change password.', 'Error');
        }
      },
      error: () => {
        this.isSaving = false;
        this.notification.error('An error occurred.', 'Error');
      }
    });
  }
}
