import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SeatClassService, SeatClassModel } from '../../../../core/services/api/seat-class.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-admin-seat-classes',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-seat-classes.component.html',
  styleUrls: ['./admin-seat-classes.component.css']
})
export class AdminSeatClassesComponent implements OnInit {
  items: SeatClassModel[] = [];
  filtered: SeatClassModel[] = [];
  isLoading = true;
  searchQuery = '';

  // Modal state
  isModalOpen = false;
  editingItem: SeatClassModel | null = null;
  model: Partial<SeatClassModel> = {
    seatClassName: ''
  };

  constructor(
    private svc: SeatClassService,
    private notification: NotificationService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.isLoading = true;
    this.svc.getAll().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.items = (res.data || []).map((item: any) => ({
            ...item,
            seatClassId: item.seatClassId || item.SeatClassID || item.seatClassID || 0
          }));
          this.applyFilter();
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('Failed to load seat classes');
      }
    });
  }

  applyFilter() {
    const q = this.searchQuery.toLowerCase();
    this.filtered = q ? this.items.filter(i =>
      i.seatClassName.toLowerCase().includes(q)
    ) : [...this.items];
  }

  openModal(item?: SeatClassModel) {
    if (item) {
      this.editingItem = item;
      this.model = { ...item };
    } else {
      this.editingItem = null;
      this.model = {
        seatClassName: ''
      };
    }
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  save() {
    if (!this.model.seatClassName) {
      this.notification.error('Seat Class Name is required', 'Validation');
      return;
    }

    const payload: any = {
      seatClassName: this.model.seatClassName
    };

    const editId = this.editingItem
      ? (this.editingItem.seatClassId || (this.editingItem as any).SeatClassID || (this.editingItem as any).seatClassID)
      : null;

    const request = editId
      ? this.svc.update(editId, payload)
      : this.svc.create(payload);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Seat Class saved successfully');
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
    if (confirm('Are you sure you want to delete this seat class?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Seat Class deleted');
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
