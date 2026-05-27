import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RefundService } from '../../../core/services/api/refund.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-my-refunds',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './my-refunds.component.html',
  styleUrls: ['./my-refunds.component.css']
})
export class MyRefundsComponent implements OnInit {
  refunds: any[] = [];
  isLoading = true;

  // OTP modal state
  showOtpModal = false;
  selectedRefund: any = null;
  otpCode = '';
  isVerifying = false;

  constructor(
    private refundService: RefundService,
    private authService: AuthService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadRefunds();
  }

  loadRefunds() {
    this.isLoading = true;
    this.refundService.getByUser(0).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.refunds = res.data || [];
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  openOtpModal(refund: any) {
    this.selectedRefund = refund;
    this.otpCode = '';
    this.showOtpModal = true;
  }

  closeOtpModal() {
    this.showOtpModal = false;
    this.selectedRefund = null;
    this.otpCode = '';
  }

  verifyOtp() {
    if (!this.otpCode || this.otpCode.length !== 6) {
      this.notification.warning('Please enter the 6-digit OTP from your email.');
      return;
    }

    this.isVerifying = true;
    this.refundService.verifyRefundOtp(this.selectedRefund.refundID, this.otpCode).subscribe({
      next: (res: any) => {
        this.isVerifying = false;
        if (res.success) {
          this.notification.success('Your refund has been successfully verified and processed!', 'Refund Completed!');
          this.closeOtpModal();
          this.loadRefunds(); // Refresh the list
        } else {
          this.notification.error(res.message || 'Invalid or expired OTP. Please try again.', 'Verification Failed');
        }
      },
      error: (err: any) => {
        this.isVerifying = false;
        const msg = err?.error?.message || 'Invalid or expired OTP.';
        this.notification.error(msg, 'Verification Failed');
      }
    });
  }

  getStatusClass(statusId: number, requiresOtp: boolean): string {
    if (requiresOtp) return 'badge-warning';
    const map: Record<number, string> = {
      1: 'badge-warning',   // Pending
      2: 'badge-info',      // Approved (OTP verified)
      3: 'badge-error',     // Rejected
      4: 'badge-success'    // Completed
    };
    return map[statusId] || 'badge-ghost';
  }

  getStatusLabel(statusName: string, requiresOtp: boolean): string {
    if (requiresOtp) return 'OTP Pending';
    return statusName || 'Unknown';
  }

  formatDate(d: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
