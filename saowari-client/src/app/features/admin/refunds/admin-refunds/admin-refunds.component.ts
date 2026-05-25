import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RefundService } from '../../../../core/services/api/refund.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-refunds',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-refunds.component.html',
  styleUrls: ['./admin-refunds.component.css']
})
export class AdminRefundsComponent implements OnInit {
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
    
    // Status ID mappings: 1 = Pending, 2 = Approved, 3 = Rejected, 4 = Processed
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
        if (res.success) { this.notification.success('Refund status updated.'); this.closeStatusModal(); this.load(); }
        else this.notification.error(res.message || 'Failed.');
      },
      error: (err: any) => this.notification.error(err.error?.message || 'Error updating status')
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = { 'Pending': 'badge-warning', 'Approved': 'badge-success', 'Rejected': 'badge-error', 'Processed': 'badge-info' };
    return map[status] || 'badge-ghost';
  }

  getStatusCount(statusId: number): number {
    return this.items.filter(i => (i.refundStatusId || i.refundStatusID || 1) === statusId).length;
  }
}

