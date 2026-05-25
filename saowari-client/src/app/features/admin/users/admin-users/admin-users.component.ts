import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/api/user.service';
import { UserRoleService } from '../../../../core/services/api/user-role.service';
import { CompanyService } from '../../../../core/services/api/company.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css']
})
export class AdminUsersComponent implements OnInit {
  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;
  searchQuery = '';
  selectedRole = '';
  showModal = false;
  editingItem: any = null;
  isSaving = false;
  rolesList: any[] = [];
  companiesList: any[] = [];

  constructor(
    private userService: UserService,
    private roleService: UserRoleService,
    private companyService: CompanyService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void { this.load(); }

  load() {
    this.isLoading = true;
    this.userService.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { this.items = res.data || []; this.applyFilter(); }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });

    this.roleService.getAll().subscribe(res => {
      if (res.success) this.rolesList = res.data || [];
    });

    this.companyService.getAll().subscribe(res => {
      if (res.success) this.companiesList = res.data || [];
    });
  }

  applyFilter() {
    let data = [...this.items];
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      data = data.filter(i => i.fullName?.toLowerCase().includes(q) || i.email?.toLowerCase().includes(q) || i.phone?.includes(q));
    }
    if (this.selectedRole) data = data.filter(i => i.roleName === this.selectedRole);
    this.filtered = data;
  }

  openAdd() {
    this.editingItem = { 
      fullName: '', 
      email: '', 
      phone: '', 
      password: '',
      roleID: null, 
      companyId: null,
      licenceNumber: '',
      licenceExpDate: '',
      isActive: true 
    }; 
    this.showModal = true; 
  }

  get isEditing(): boolean {
    return !!(this.editingItem?.userId || this.editingItem?.userID);
  }

  openEdit(item: any) {
    this.editingItem = { ...item };
    this.editingItem.roleID = item.roleID || item.roleId;
    if(this.editingItem.licenceExpDate) {
       this.editingItem.licenceExpDate = new Date(this.editingItem.licenceExpDate).toISOString().split('T')[0];
    }
    this.showModal = true; 
  }

  closeModal() { this.showModal = false; this.editingItem = null; }

  get isDriverSelected(): boolean {
    if (!this.editingItem?.roleID) return false;
    const role = this.rolesList.find(r => r.userRoleId === this.editingItem.roleID || r.id === this.editingItem.roleID);
    return role?.userRoleName === 'Driver';
  }

  get isCustomerSelected(): boolean {
    if (!this.editingItem?.roleID) return false;
    const role = this.rolesList.find(r => r.userRoleId === this.editingItem.roleID || r.id === this.editingItem.roleID);
    return role?.userRoleName === 'Customer';
  }

  save() {
    if (!this.editingItem.fullName || !this.editingItem.email || !this.editingItem.phone || !this.editingItem.roleID) {
      this.notification.error('Please fill all required fields.', 'Validation');
      return;
    }
    
    // Removed password requirement since it's auto-generated if empty

    if (this.isDriverSelected) {
        if (!this.editingItem.licenceNumber || !this.editingItem.licenceExpDate) {
            this.notification.error('Licence Number and Expiry Date are required for Drivers.', 'Validation');
            return;
        }
    }

    this.isSaving = true;

    // Formatting date if needed
    const payload = { ...this.editingItem };
    if (payload.licenceExpDate) {
        payload.licenceExpDate = new Date(payload.licenceExpDate).toISOString();
    }

    const id = this.editingItem.userId || this.editingItem.userID;
    const obs = id
      ? this.userService.update(id, payload)
      : this.userService.create(payload);
      
    obs.subscribe({
      next: (res: any) => {
        this.isSaving = false;
        if (res.success) { this.notification.success('User saved.'); this.load(); this.closeModal(); }
        else this.notification.error(res.message || 'Failed.');
      },
      error: () => { this.isSaving = false; this.notification.error('An error occurred.'); }
    });
  }

  toggleActive(item: any) {
    const id = item.userId || item.userID;
    this.userService.patchActive(id, !item.isActive).subscribe({
      next: (res: any) => {
        if (res.success) { item.isActive = !item.isActive; this.notification.success('Status updated.'); }
      }
    });
  }

  get uniqueRoles(): string[] {
    return [...new Set(this.items.map(i => i.roleName).filter(Boolean))] as string[];
  }

  expandedPictureUrl: string | null = null;

  getProfilePictureUrl(path: string | null | undefined): string {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('data:')) {
      return path;
    }
    const cleanPath = path.startsWith('/') ? path : '/' + path;
    return 'http://localhost:5293' + cleanPath;
  }

  expandPicture(url: string | null | undefined): void {
    if (url) {
      this.expandedPictureUrl = this.getProfilePictureUrl(url);
    }
  }

  closeExpandedPicture(): void {
    this.expandedPictureUrl = null;
  }
}
