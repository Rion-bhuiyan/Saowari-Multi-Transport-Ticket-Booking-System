import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RefundService } from '../../../../core/services/api/refund.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-refunds',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-refunds.component.html',
  styleUrls: ['./admin-refunds.component.css']
})
export class AdminRefundsComponent implements OnInit {
  get pagedItems() {
    const start = (this.p - 1) * Number(this.pageSize);
    return (this.filtered || this.items || []).slice(start, start + Number(this.pageSize));
  }
  p: number = 1;
  pageSize: number = 15;

  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';
  isStatusModalOpen = false;
  selectedItem: any = null;
  newStatusId: number = 1;
  activeTab: 'pending' | 'approved' | 'rejected' | 'processed' = 'pending';

  readonly statuses = [
    { id: 1, name: 'Pending' },
    { id: 2, name: 'Approved' },
    { id: 3, name: 'Rejected' },
    { id: 4, name: 'Processed' }
  ];

  constructor(
    private svc: RefundService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void { this.load(); }

  load() {
    this.isLoading = true;
    this.svc.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { this.items = res.data || []; this.applyFilter(); }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  setActiveTab(tab: 'pending' | 'approved' | 'rejected' | 'processed') {
    this.activeTab = tab;
    this.applyFilter();
  }

  applyFilter() {
    const q = this.searchQuery.toLowerCase();

    const statusMap: Record<string, number> = {
      'pending': 1,
      'approved': 2,
      'rejected': 3,
      'processed': 4
    };
    const targetStatusId = statusMap[this.activeTab];

    const tabFiltered = this.items.filter(i => {
      const sId = i.refundStatusId || i.refundStatusID || 1;
      return sId === targetStatusId;
    });

    this.filtered = q ? tabFiltered.filter(i => JSON.stringify(i).toLowerCase().includes(q)) : [...tabFiltered];
  }

  openStatusModal(item: any) {
    this.selectedItem = item;
    this.newStatusId = item.refundStatusId || item.refundStatusID || 1;
    this.isStatusModalOpen = true;
  }

  closeStatusModal() { this.isStatusModalOpen = false; }

  updateStatus() {
    if (!this.selectedItem) return;
    const id = this.selectedItem.refundID || this.selectedItem.refundId || this.selectedItem.id;
    this.svc.patchStatus(id, this.newStatusId).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Refund status updated.');
          this.closeStatusModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed.');
        }
      },
      error: (err: any) => this.notification.error(err.error?.message || 'Error updating status')
    });
  }

  resetToOtpPending(item: any) {
    const id = item.refundID || item.refundId || item.id;
    if (!confirm(`Reset Refund #${id} back to OTP-Pending so the customer can verify their OTP?`)) return;
    this.svc.resetToOtpPending(id).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Refund reset. Customer can now enter their OTP.', 'Reset Successful');
          this.load();
        } else {
          this.notification.error(res.message || 'Reset failed.', 'Error');
        }
      },
      error: (err: any) => this.notification.error(err?.error?.message || 'Error resetting refund.')
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'badge-warning',
      'Approved': 'badge-success',
      'Rejected': 'badge-error',
      'Processed': 'badge-info'
    };
    return map[status] || 'badge-ghost';
  }

  getStatusCount(statusId: number): number {
    return this.items.filter(i => (i.refundStatusId || i.refundStatusID || 1) === statusId).length;
  }
}
