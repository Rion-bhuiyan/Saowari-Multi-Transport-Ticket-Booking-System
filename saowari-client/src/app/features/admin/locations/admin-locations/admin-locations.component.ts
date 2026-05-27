import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LocationService } from '../../../../core/services/api/location.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LocationModel } from '../../../../core/models/master.model';

@Component({
  selector: 'app-admin-locations',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-locations.component.html',
  styleUrls: ['./admin-locations.component.css']
})
export class AdminLocationsComponent implements OnInit {
  get pagedItems() {
    const start = (this.p - 1) * Number(this.pageSize);
    return (this.filtered || this.items || []).slice(start, start + Number(this.pageSize));
  }
  p: number = 1;
  pageSize: number = 15;

  items: LocationModel[] = [];
  filtered: LocationModel[] = [];
  isLoading = true;
  searchQuery = '';

  // Modal state
  isModalOpen = false;
  editingItem: LocationModel | null = null;
  model: Partial<LocationModel> = { 
    locationName: '', 
    locationCode: undefined, 
    latitude: undefined, 
    longitude: '', 
    district: '', 
    isActive: true 
  };

  constructor(
    private svc: LocationService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void { 
    this.load(); 
  }

  load() {
    this.isLoading = true;
    this.svc.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { 
          // Normalize the ID field from PascalCase (LocationID) to camelCase (locationId)
          this.items = (res.data || []).map((item: any) => ({
            ...item,
            locationId: item.locationId || item.LocationID || item.locationID || 0
          }));
          this.applyFilter(); 
        }
        this.isLoading = false;
      },
      error: () => { 
        this.isLoading = false; 
        this.notification.error('Failed to load locations');
      }
    });
  }

  applyFilter() {
    const q = this.searchQuery.toLowerCase();
    this.filtered = q ? this.items.filter(i =>
      i.locationName.toLowerCase().includes(q)
    ) : [...this.items];
  }

  openModal(item?: LocationModel) {
    if (item) {
      this.editingItem = item;
      this.model = { ...item };
    } else {
      this.editingItem = null;
      this.model = { 
        locationName: '', 
        locationCode: undefined, 
        latitude: undefined, 
        longitude: '', 
        district: '', 
        isActive: true 
      };
    }
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  save() {
    if (!this.model.locationName) {
      this.notification.error('Location Name is required', 'Validation');
      return;
    }

    const payload: any = {
      locationName: this.model.locationName,
      locationCode: this.model.locationCode ? Number(this.model.locationCode) : 0,
      latitude: this.model.latitude != null && this.model.latitude !== (undefined as any) ? Number(this.model.latitude) : null,
      longitude: this.model.longitude != null ? String(this.model.longitude) : null,
      district: this.model.district || null,
      isActive: this.model.isActive ?? true
    };

    const editId = this.editingItem 
      ? (this.editingItem.locationId || (this.editingItem as any).LocationID || (this.editingItem as any).locationID)
      : null;

    const request = editId
      ? this.svc.update(editId, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Location saved successfully');
          this.closeModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed to save');
        }
      },
      error: () => this.notification.error('An error occurred')
    });
  }

  deleteItem(id: number) {
    if (confirm('Are you sure you want to delete this location?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Location deleted');
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
