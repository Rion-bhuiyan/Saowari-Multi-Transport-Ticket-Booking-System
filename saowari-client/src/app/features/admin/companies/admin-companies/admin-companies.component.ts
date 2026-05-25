import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CompanyService } from '../../../../core/services/api/company.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CompanyTypeService } from '../../../../core/services/api/company-type.service';

@Component({
  selector: 'app-admin-companies',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-companies.component.html',
  styleUrls: ['./admin-companies.component.css']
})
export class AdminCompaniesComponent implements OnInit {
  items: any[] = [];
  filtered: any[] = [];
  companyTypes: any[] = [];
  isLoading = true;
  searchQuery = '';

  // Modal state
  isModalOpen = false;
  editingItem: any = null;
  model: any = { companyName: '', contactEmail: '', contactPhone: '', companyTypeId: null, isActive: true };
  selectedFile: File | null = null;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(
    private svc: CompanyService,
    private typeSvc: CompanyTypeService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void { 
    this.load(); 
    this.loadTypes();
  }

  loadTypes() {
    this.typeSvc.getAll().subscribe({
      next: (res: any) => { if (res.success) this.companyTypes = res.data || []; }
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
      this.model = { ...item };
      this.imagePreview = item.logoURL || null;
    } else {
      this.editingItem = null;
      this.model = { companyName: '', contactEmail: '', contactPhone: '', companyTypeId: null, isActive: true };
      this.imagePreview = null;
    }
    this.selectedFile = null;
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
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

  save() {
    if (!this.model.companyName || !this.model.contactEmail || !this.model.contactPhone || !this.model.companyTypeId) {
      this.notification.error('Please fill all required fields', 'Validation');
      return;
    }

    const formData = new FormData();
    formData.append('companyName', this.model.companyName);
    formData.append('contactEmail', this.model.contactEmail);
    formData.append('contactPhone', this.model.contactPhone);
    formData.append('companyTypeId', this.model.companyTypeId.toString());
    formData.append('isActive', this.model.isActive ? 'true' : 'false');
    
    if (this.selectedFile) {
      formData.append('logoFile', this.selectedFile);
    }

    const request = this.editingItem 
      ? this.svc.update(this.editingItem.companyID || this.editingItem.companyId || this.editingItem.id, formData)
      : this.svc.create(formData);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Company saved successfully');
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
    if (confirm('Are you sure you want to delete this company?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Company deleted');
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

