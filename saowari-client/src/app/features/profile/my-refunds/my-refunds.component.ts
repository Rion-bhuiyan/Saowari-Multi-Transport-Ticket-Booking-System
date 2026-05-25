import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { RefundService } from '../../../core/services/api/refund.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-my-refunds',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-refunds.component.html',
  styleUrls: ['./my-refunds.component.css']
})
export class MyRefundsComponent implements OnInit {
  refunds: any[] = [];
  isLoading = true;

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
    this.authService.currentUser$.subscribe(user => {
      if (user?.userId) {
        this.refundService.getByUser(user.userId).subscribe({
          next: (res: any) => {
            if (res.success) {
              this.refunds = res.data || [];
            }
            this.isLoading = false;
          },
          error: () => { this.isLoading = false; }
        });
      } else {
        this.isLoading = false;
      }
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Approved': 'badge-success',
      'Pending': 'badge-warning',
      'Rejected': 'badge-error',
      'Processing': 'badge-info'
    };
    return map[status] || 'badge-ghost';
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
