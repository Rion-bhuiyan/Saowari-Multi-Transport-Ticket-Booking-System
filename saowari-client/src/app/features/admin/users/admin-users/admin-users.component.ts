import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import * as signalR from '@microsoft/signalr';
import { UserService } from '../../../../core/services/api/user.service';
import { UserRoleService } from '../../../../core/services/api/user-role.service';
import { CompanyService } from '../../../../core/services/api/company.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { environment } from '../../../../../environments/environment';
import { TokenService } from '../../../../core/services/token.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css']
})
export class AdminUsersComponent implements OnInit, OnDestroy {
  items: any[] = [];
  filtered: any[] = [];
  isLoading = true;

  // Filters
  searchQuery = '';
  selectedRole = '';
  selectedCompany = '';

  // Sorting
  sortField: 'createdAt' | 'fullName' | 'roleName' | 'companyName' | 'isOnline' = 'createdAt';
  sortDir: 'desc' | 'asc' = 'desc';

  // Modal
  showModal = false;
  editingItem: any = null;
  isSaving = false;

  // Lookup lists
  rolesList: any[] = [];
  companiesList: any[] = [];

  // Picture expand
  expandedPictureUrl: string | null = null;

  // SignalR
  private hubConnection: signalR.HubConnection | null = null;

  constructor(
    private userService: UserService,
    private roleService: UserRoleService,
    private companyService: CompanyService,
    private notification: NotificationService,
    private tokenService: TokenService
  ) {}

  ngOnInit(): void { 
    this.load(); 
    this.initSignalR();
  }

  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  initSignalR() {
    const hubUrl = environment.apiUrl.replace('/api', '/presenceHub');
    const token = this.tokenService.getAccessToken();
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token || '' })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('UserIsOnline', (userId: number) => {
      const user = this.items.find(u => u.userID === userId);
      if (user) {
        user.isOnline = true;
        this.applyFilter();
      }
    });

    this.hubConnection.on('UserIsOffline', (userId: number) => {
      const user = this.items.find(u => u.userID === userId);
      if (user) {
        user.isOnline = false;
        this.applyFilter();
      }
    });

    this.hubConnection.start()
      .catch(err => console.error('Error starting PresenceHub connection:', err));
  }

  load() {
    this.isLoading = true;
    this.userService.getAll().subscribe({
      next: (res: any) => {
        if (res.success) { this.items = res.data || []; this.applyFilter(); }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });

    this.roleService.getAll().subscribe((res: any) => {
      if (res.success) this.rolesList = res.data || [];
    });

    this.companyService.getAll().subscribe((res: any) => {
      if (res.success) this.companiesList = res.data || [];
    });
  }

  applyFilter() {
    let data = [...this.items];

    // Search
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      data = data.filter(i =>
        i.fullName?.toLowerCase().includes(q) ||
        i.email?.toLowerCase().includes(q) ||
        i.phone?.includes(q)
      );
    }

    // Role filter
    if (this.selectedRole) {
      data = data.filter(i => i.roleName === this.selectedRole);
    }

    // Company filter
    if (this.selectedCompany) {
      data = data.filter(i => i.companyName === this.selectedCompany);
    }

    // Sort
    data.sort((a, b) => {
      let valA: any;
      let valB: any;

      if (this.sortField === 'createdAt') {
        valA = a.createdAt ? new Date(a.createdAt).getTime() : 0;
        valB = b.createdAt ? new Date(b.createdAt).getTime() : 0;
      } else if (this.sortField === 'isOnline') {
        valA = a.isOnline ? 1 : 0;
        valB = b.isOnline ? 1 : 0;
      } else {
        valA = (a[this.sortField] || '').toString().toLowerCase();
        valB = (b[this.sortField] || '').toString().toLowerCase();
      }

      if (valA < valB) return this.sortDir === 'asc' ? -1 : 1;
      if (valA > valB) return this.sortDir === 'asc' ? 1 : -1;
      return 0;
    });

    this.filtered = data;
  }

  setSort(field: 'createdAt' | 'fullName' | 'roleName' | 'companyName' | 'isOnline') {
    if (this.sortField === field) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDir = field === 'createdAt' ? 'desc' : 'asc';
    }
    this.applyFilter();
  }

  clearFilters() {
    this.searchQuery = '';
    this.selectedRole = '';
    this.selectedCompany = '';
    this.sortField = 'createdAt';
    this.sortDir = 'desc';
    this.applyFilter();
  }

  get hasActiveFilters(): boolean {
    return !!(this.searchQuery || this.selectedRole || this.selectedCompany ||
              this.sortField !== 'createdAt' || this.sortDir !== 'desc');
  }

  // ─── Modal ───────────────────────────────────────────────────────────────
  openAdd() {
    this.editingItem = {
      fullName: '', email: '', phone: '', password: '', adminCopyEmail: null,
      roleID: null, companyId: null,
      licenceNumber: null, licenceExpDate: null, isActive: true
    };
    this.showModal = true;
  }

  openEdit(item: any) {
    this.editingItem = { ...item };
    this.editingItem.roleID = item.roleID || item.roleId;
    if (this.editingItem.licenceExpDate) {
      this.editingItem.licenceExpDate = new Date(this.editingItem.licenceExpDate).toISOString().split('T')[0];
    }
    this.showModal = true;
  }

  closeModal() { this.showModal = false; this.editingItem = null; }

  get isEditing(): boolean {
    return !!(this.editingItem?.userId || this.editingItem?.userID);
  }

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
    if (this.isDriverSelected) {
      if (!this.editingItem.licenceNumber || !this.editingItem.licenceExpDate) {
        this.notification.error('Licence Number and Expiry Date are required for Drivers.', 'Validation');
        return;
      }
    }

    this.isSaving = true;
    const payload = { ...this.editingItem };
    if (payload.licenceExpDate) {
      payload.licenceExpDate = new Date(payload.licenceExpDate).toISOString();
    } else {
      payload.licenceExpDate = null;
    }
    if (!payload.licenceNumber) payload.licenceNumber = null;
    if (!payload.password) payload.password = null;
    if (!payload.adminCopyEmail) payload.adminCopyEmail = null;

    const id = this.editingItem.userId || this.editingItem.userID;
    const obs = id ? this.userService.update(id, payload) : this.userService.create(payload);

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

  // ─── Computed helpers ────────────────────────────────────────────────────
  get uniqueRoles(): string[] {
    return [...new Set(this.items.map(i => i.roleName).filter(Boolean))] as string[];
  }

  get uniqueCompanies(): string[] {
    return [...new Set(this.items.map(i => i.companyName).filter(Boolean))] as string[];
  }

  get totalActive(): number { return this.items.filter(i => i.isActive).length; }
  get totalInactive(): number { return this.items.filter(i => !i.isActive).length; }

  getProfilePictureUrl(path: string | null | undefined): string {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('data:')) return path;
    const cleanPath = path.startsWith('/') ? path : '/' + path;
    return 'http://localhost:5293' + cleanPath;
  }

  getInitials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(' ');
    return parts.length >= 2 ? (parts[0][0] + parts[1][0]).toUpperCase() : parts[0][0].toUpperCase();
  }

  getRoleBadgeClass(role: string): string {
    const map: Record<string, string> = {
      'Admin': 'badge-admin',
      'Manager': 'badge-manager',
      'Driver': 'badge-driver',
      'Supervisor': 'badge-supervisor',
      'Customer': 'badge-customer',
    };
    return map[role] || 'badge-default';
  }

  expandPicture(url: string | null | undefined): void {
    if (url) this.expandedPictureUrl = this.getProfilePictureUrl(url);
  }

  closeExpandedPicture(): void { this.expandedPictureUrl = null; }
}
