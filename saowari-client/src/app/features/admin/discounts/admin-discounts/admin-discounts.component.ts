import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DiscountService } from '../../../../core/services/api/discount.service';
import { CompanyService } from '../../../../core/services/api/company.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-admin-discounts',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-discounts.component.html',
  styleUrls: ['./admin-discounts.component.css']
})
export class AdminDiscountsComponent implements OnInit {
  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';

  companiesList: any[] = [];
  discountTypesList: any[] = [];

  isModalOpen = false;
  editingItem: any = null;
  model: any = this.defaultModel();

  constructor(
    private svc: DiscountService,
    private companyService: CompanyService,
    private http: HttpClient,
    private notification: NotificationService
  ) {}

  defaultModel() {
    const today = new Date().toISOString().split('T')[0];
    const nextYear = new Date(Date.now() + 365 * 86400000).toISOString().split('T')[0];
    return {
      companyId: null,
      routeId: null,
      vehicleTypeId: null,
      discountName: '',
      couponCode: '',
      discountTypeId: null,
      discountValue: 0,
      minTicketAmount: null,
      startDate: today,
      endDate: nextYear,
      isActive: true
    };
  }

  ngOnInit(): void {
    this.load();
    this.companyService.getAll().subscribe((res: any) => { if (res.success) this.companiesList = res.data || []; });
    this.http.get<any>(`${environment.apiUrl}/discounttypes`).subscribe(res => {
      if (res.success) this.discountTypesList = res.data || [];
    });
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
    if (item) {
      this.editingItem = item;
      this.model = {
        companyId:       item.companyID    || item.companyId,
        routeId:         item.routeID      || item.routeId      || null,
        vehicleTypeId:   item.vehicleTypeID|| item.vehicleTypeId|| null,
        discountName:    item.discountName,
        couponCode:      item.couponCode   || '',
        discountTypeId:  item.discountTypeID || item.discountTypeId,
        discountValue:   item.discountValue,
        minTicketAmount: item.minTicketAmount || null,
        startDate:       item.startDate ? new Date(item.startDate).toISOString().split('T')[0] : '',
        endDate:         item.endDate   ? new Date(item.endDate).toISOString().split('T')[0]   : '',
        isActive:        item.isActive !== undefined ? item.isActive : true
      };
    } else {
      this.editingItem = null;
      this.model = this.defaultModel();
    }
    this.isModalOpen = true;
  }

  closeModal() { this.isModalOpen = false; }

  save() {
    if (!this.model.discountName || !this.model.companyId || !this.model.discountTypeId ||
        this.model.discountValue <= 0 || !this.model.startDate || !this.model.endDate) {
      this.notification.error('Please fill all required fields', 'Validation Error');
      return;
    }
    if (new Date(this.model.endDate) <= new Date(this.model.startDate)) {
      this.notification.error('End date must be after start date', 'Validation Error');
      return;
    }

    const payload: any = {
      companyId:      Number(this.model.companyId),
      discountName:   this.model.discountName,
      couponCode:     this.model.couponCode || null,
      discountTypeId: Number(this.model.discountTypeId),
      discountValue:  Number(this.model.discountValue),
      startDate:      new Date(this.model.startDate).toISOString(),
      endDate:        new Date(this.model.endDate).toISOString(),
      isActive:       this.model.isActive
    };
    if (this.model.routeId)         payload.routeId         = Number(this.model.routeId);
    if (this.model.vehicleTypeId)   payload.vehicleTypeId   = Number(this.model.vehicleTypeId);
    if (this.model.minTicketAmount) payload.minTicketAmount = Number(this.model.minTicketAmount);

    const id = this.editingItem?.discountID || this.editingItem?.discountId || this.editingItem?.id;
    const request = this.editingItem
      ? this.svc.update(id, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success(this.editingItem ? 'Discount updated successfully' : 'Discount created successfully');
          this.closeModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed to save discount');
        }
      },
      error: (err: any) => {
        const msg = err?.error?.message || 'An error occurred';
        this.notification.error(msg);
      }
    });
  }

  deleteItem(id: number) {
    if (!id) { this.notification.error('Invalid ID'); return; }
    if (confirm('Are you sure you want to delete this discount?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) { this.notification.success('Discount deleted.'); this.load(); }
          else this.notification.error(res.message || 'Failed to delete.');
        },
        error: () => this.notification.error('An error occurred while deleting')
      });
    }
  }

  getCompanyName(id: number): string {
    const c = this.companiesList.find(c => (c.companyID || c.companyId || c.id) === id);
    return c ? c.companyName : `Company ${id}`;
  }

  getDiscountTypeName(id: number): string {
    const t = this.discountTypesList.find(t => (t.discountTypeID || t.discountTypeId || t.id) === id);
    return t ? (t.discountTypeName || t.name) : `Type ${id}`;
  }
}
