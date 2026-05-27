import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserRoleService } from '../../../../core/services/api/user-role.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-roles',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-roles.component.html',
  styleUrls: ['./admin-roles.component.css']
})
export class AdminRolesComponent implements OnInit {
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

  isModalOpen = false;
  editingItem: any = null;
  model: any = { userRoleName: '' };

  constructor(
    private svc: UserRoleService,
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
    this.filtered = q ? this.items.filter(i =>
      (i.userRoleName || '').toLowerCase().includes(q)
    ) : [...this.items];
  }

  /** Resolves ID regardless of PascalCase or camelCase from backend */
  getRoleId(item: any): number {
    return item.userRoleId || item.userRoleID || item.id;
  }

  openModal(item?: any) {
    if (item) {
      this.editingItem = item;
      this.model = { userRoleName: item.userRoleName };
    } else {
      this.editingItem = null;
      this.model = { userRoleName: '' };
    }
    this.isModalOpen = true;
  }

  closeModal() { this.isModalOpen = false; }

  save() {
    if (!this.model.userRoleName?.trim()) {
      this.notification.error('Role name is required', 'Validation');
      return;
    }

    const payload = { userRoleName: this.model.userRoleName.trim() };
    const id = this.editingItem ? this.getRoleId(this.editingItem) : null;

    if (!id && this.editingItem) {
      this.notification.error('Cannot update: role ID not found');
      return;
    }

    const request = this.editingItem
      ? this.svc.update(id!, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success(this.editingItem ? 'Role updated successfully' : 'Role created successfully');
          this.closeModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed to save role');
        }
      },
      error: (err: any) => {
        const msg = err?.error?.message || 'An error occurred';
        this.notification.error(msg);
      }
    });
  }

  deleteItem(id: number) {
    if (!id) { this.notification.error('Cannot delete: invalid role ID'); return; }
    if (confirm('Are you sure you want to delete this role?\nUsers assigned to this role may be affected.')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Role deleted successfully');
            this.load();
          } else {
            this.notification.error(res.message || 'Failed to delete role');
          }
        },
        error: () => this.notification.error('An error occurred while deleting')
      });
    }
  }
}
