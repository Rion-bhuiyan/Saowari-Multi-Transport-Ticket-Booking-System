import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RouteService } from '../../../../core/services/api/route.service';
import { LocationService } from '../../../../core/services/api/location.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-routes',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-routes.component.html',
  styleUrls: ['./admin-routes.component.css']
})
export class AdminRoutesComponent implements OnInit {
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

  locations: any[] = [];

  // Modal state
  isModalOpen = false;
  editingItem: any = null;
  model: any = { routeName: '', fromLocationId: null, toLocationId: null, distance: 0, estimatedDuration: '00:00:00', isActive: true, imageUrl: '' };
  
  fromSearchQuery = '';
  toSearchQuery = '';

  get filteredFromLocations() {
    if (!this.fromSearchQuery) return this.locations;
    return this.locations.filter(l => l.locationName.toLowerCase().includes(this.fromSearchQuery.toLowerCase()));
  }

  get filteredToLocations() {
    if (!this.toSearchQuery) return this.locations;
    return this.locations.filter(l => l.locationName.toLowerCase().includes(this.toSearchQuery.toLowerCase()));
  }

  selectFromLocation(loc: any) {
    this.model.fromLocationId = loc.locationID || loc.locationId || loc.id;
  }

  selectToLocation(loc: any) {
    this.model.toLocationId = loc.locationID || loc.locationId || loc.id;
  }
  
  imagePreview: string | ArrayBuffer | null = null;
  selectedFile: File | null = null;

  constructor(
    private svc: RouteService,
    private locationService: LocationService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void { 
    this.load(); 
    this.locationService.getAll().subscribe(res => { if (res.success) this.locations = res.data; });
  }

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
      JSON.stringify(i).toLowerCase().includes(q)
    ) : [...this.items];
  }

  openModal(item?: any) {
    this.removeSelection();
    if (item) {
      this.editingItem = item;
      this.model = { 
        ...item,
        fromLocationId: item.fromLocationID || item.fromLocationId,
        toLocationId: item.toLocationID || item.toLocationId,
        distance: item.distanceKM || item.distance,
        estimatedDuration: this.formatHoursToDuration(item.estimatedHours || item.estimatedDuration),
        routeName: item.routeName || '',
        imageUrl: item.imageUrl || '',
      };
      if (this.model.imageUrl) {
        this.imagePreview = this.model.imageUrl;
      }
    } else {
      this.editingItem = null;
      this.model = { routeName: '', fromLocationId: null, toLocationId: null, distance: 0, estimatedDuration: '00:00:00', isActive: true, imageUrl: '' };
    }
    this.isModalOpen = true;
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = (e) => this.imagePreview = reader.result;
      reader.readAsDataURL(file);
    }
  }

  removeSelection() {
    this.selectedFile = null;
    this.imagePreview = null;
    this.model.imageUrl = '';
  }

  formatHoursToDuration(decimalHours: any): string {
    if (!decimalHours) return '00:00:00';
    const num = Number(decimalHours);
    if (isNaN(num)) return String(decimalHours);
    
    const h = Math.floor(num);
    const m = Math.round((num - h) * 60);
    const hh = h.toString().padStart(2, '0');
    const mm = m.toString().padStart(2, '0');
    return `${hh}:${mm}:00`;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  save() {
    if (!this.model.fromLocationId || !this.model.toLocationId) {
      this.notification.error('Please fill all required fields', 'Validation');
      return;
    }
    
    if (this.model.fromLocationId === this.model.toLocationId) {
      this.notification.error('From and To locations cannot be the same', 'Validation');
      return;
    }

    // Convert estimatedDuration (e.g. 08:00:00 or 8.5) to decimal hours
    let hours = 0;
    if (this.model.estimatedDuration) {
      const parts = this.model.estimatedDuration.split(':');
      if (parts.length >= 2) {
        hours = parseInt(parts[0]) + (parseInt(parts[1]) / 60);
      } else {
        hours = parseFloat(this.model.estimatedDuration);
      }
    }

    const payload = new FormData();
    payload.append('fromLocationID', this.model.fromLocationId.toString());
    payload.append('toLocationID', this.model.toLocationId.toString());
    payload.append('distanceKM', this.model.distance.toString());
    payload.append('estimatedHours', hours.toString());
    payload.append('isActive', this.model.isActive.toString());

    if (this.selectedFile) {
      payload.append('imageFile', this.selectedFile);
    }
    
    const request = this.editingItem 
      ? this.svc.update(this.editingItem.routeID || this.editingItem.routeId || this.editingItem.id, payload as any)
      : this.svc.create(payload as any);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Route saved successfully');
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
    const confirmed = confirm(
      '⚠️ WARNING: Deleting this route will also permanently delete ALL linked schedules, bookings, payments, and tickets.\n\nAre you sure you want to proceed?'
    );
    if (confirmed) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Route and all related data deleted successfully');
            this.load();
          } else {
            this.notification.error(res.message || 'Failed to delete');
          }
        },
        error: (err) => {
          const errMsg = err?.error?.message || 'An error occurred while deleting';
          this.notification.error(errMsg);
        }
      });
    }
  }

  getLocationName(id: number): string {
    if (!id) return '';
    const loc = this.locations.find(l => (l.locationID || l.locationId || l.id) === id);
    return loc ? loc.locationName : id.toString();
  }

  getRouteName(item: any): string {
    if (item.routeName) return item.routeName;
    const from = this.getLocationName(item.fromLocationID || item.fromLocationId);
    const to = this.getLocationName(item.toLocationID || item.toLocationId);
    if (from && to) {
      return `${from} to ${to}`;
    }
    return `Route ${item.routeID || item.routeId || item.id}`;
  }

  getFormRouteName(): string {
    if (!this.model.fromLocationId || !this.model.toLocationId) return '';
    const from = this.getLocationName(this.model.fromLocationId);
    const to = this.getLocationName(this.model.toLocationId);
    return `${from} to ${to}`;
  }
}

