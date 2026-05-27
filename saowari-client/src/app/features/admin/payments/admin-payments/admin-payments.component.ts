import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../../../core/services/api/payment.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-payments',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-payments.component.html',
  styleUrls: ['./admin-payments.component.css']
})
export class AdminPaymentsComponent implements OnInit {
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
  selectedFilter = '';
  isStatusModalOpen = false;
  selectedItem: any = null;
  newStatusId: number = 1;

  readonly statuses = [
    { id: 1, name: 'Pending' },
    { id: 2, name: 'Completed' },
    { id: 3, name: 'Failed' },
    { id: 4, name: 'Refunded' }
  ];

  constructor(
    private svc: PaymentService,
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

  applyFilter() {
    const q = this.searchQuery.toLowerCase();
    let data = q ? this.items.filter(i => JSON.stringify(i).toLowerCase().includes(q)) : [...this.items];
    if (this.selectedFilter) data = data.filter(i => (i.paymentStatusName || i.paymentStatusId?.toString()) === this.selectedFilter);
    this.filtered = data;
  }

  openStatusModal(item: any) {
    this.selectedItem = item;
    this.newStatusId = item.paymentStatusId || 1;
    this.isStatusModalOpen = true;
  }

  closeStatusModal() { this.isStatusModalOpen = false; }

  updateStatus() {
    if (!this.selectedItem) return;
    const id = this.selectedItem.paymentId || this.selectedItem.id;
    this.svc.patchStatus(id, this.newStatusId).subscribe({
      next: (res: any) => {
        if (res.success) { this.notification.success('Payment status updated.'); this.closeStatusModal(); this.load(); }
        else this.notification.error(res.message || 'Failed.');
      },
      error: () => this.notification.error('Error updating status')
    });
  }

  getStatusClass(status: string | number): string {
    const map: Record<string, string> = { 'Pending': 'badge-warning', 'Completed': 'badge-success', 'Failed': 'badge-error', 'Refunded': 'badge-info' };
    return map[status] || 'badge-ghost';
  }
}

