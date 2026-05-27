import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VehicleTypeService } from '../../../../core/services/api/vehicle-type.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-admin-vehicle-types',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './admin-vehicle-types.component.html'
})
export class AdminVehicleTypesComponent implements OnInit {
  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';

  p: number = 1;
  pageSize: number = 15;

  isModalOpen = false;
  editingItem: any = null;
  model: any = { vehicleTypeName: '' };

  constructor(
    private svc: VehicleTypeService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get pagedItems() {
    const start = (this.p - 1) * Number(this.pageSize);
    return (this.filtered || []).slice(start, start + Number(this.pageSize));
  }

  load() {
    this.isLoading = true;
    this.svc.getAll().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.items = res.data || [];
          this.applyFilter();
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('Failed to load vehicle types');
      }
    });
  }

  applyFilter() {
    const q = this.searchQuery.toLowerCase();
    this.filtered = q ? this.items.filter(i =>
      (i.vehicleTypeName || '').toLowerCase().includes(q)
    ) : [...this.items];
  }

  openModal(item?: any) {
    if (item) {
      this.editingItem = item;
      this.model = { vehicleTypeName: item.vehicleTypeName };
    } else {
      this.editingItem = null;
      this.model = { vehicleTypeName: '' };
    }
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  save() {
    if (!this.model.vehicleTypeName) {
      this.notification.error('Vehicle Type Name is required');
      return;
    }

    const payload = {
      vehicleTypeName: this.model.vehicleTypeName
    };

    const request = this.editingItem
      ? this.svc.update(this.editingItem.vehicleTypeId || this.editingItem.id, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success(this.editingItem ? 'Vehicle Type updated successfully' : 'Vehicle Type created successfully');
          this.closeModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed to save');
        }
      },
      error: () => this.notification.error('An error occurred while saving')
    });
  }

  deleteItem(id: number) {
    if (confirm('Are you sure you want to delete this vehicle type?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Vehicle Type deleted');
            this.load();
          } else {
            this.notification.error(res.message || 'Failed to delete');
          }
        },
        error: () => this.notification.error('An error occurred while deleting')
      });
    }
  }
}
