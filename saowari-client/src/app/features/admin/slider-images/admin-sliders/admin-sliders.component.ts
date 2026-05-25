import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SliderImageService } from '../../../../core/services/api/slider-image.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { SliderImageModel } from '../../../../core/models/master.model';

@Component({
  selector: 'app-admin-sliders',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-sliders.component.html',
  styleUrls: ['./admin-sliders.component.css']
})
export class AdminSlidersComponent implements OnInit {
  items: SliderImageModel[] = [];
  filtered: SliderImageModel[] = [];
  isLoading = true;
  searchQuery = '';

  // Modal & Form state
  isModalOpen = false;
  editingItem: SliderImageModel | null = null;
  selectedFile: File | null = null;
  imagePreviewUrl: string | null = null;

  model = {
    title: '',
    subtitle: '',
    linkUrl: '',
    displayOrder: 0,
    isActive: true
  };

  constructor(
    private svc: SliderImageService,
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
          // Normalize IDs to handle potential pascal case response fields
          this.items = (res.data || []).map((item: any) => ({
            ...item,
            sliderImageID: item.sliderImageID || item.SliderImageID || 0
          }));
          this.applyFilter();
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notification.error('Failed to load slider images');
      }
    });
  }

  applyFilter() {
    const q = this.searchQuery.toLowerCase();
    this.filtered = q ? this.items.filter(i =>
      (i.title || '').toLowerCase().includes(q) ||
      (i.subtitle || '').toLowerCase().includes(q)
    ) : [...this.items];
  }

  openModal(item?: SliderImageModel) {
    this.selectedFile = null;
    this.imagePreviewUrl = null;

    if (item) {
      this.editingItem = item;
      this.model = {
        title: item.title || '',
        subtitle: item.subtitle || '',
        linkUrl: item.linkUrl || '',
        displayOrder: item.displayOrder ?? 0,
        isActive: item.isActive ?? true
      };
      this.imagePreviewUrl = item.imageUrl;
    } else {
      this.editingItem = null;
      this.model = {
        title: '',
        subtitle: '',
        linkUrl: '',
        displayOrder: 0,
        isActive: true
      };
    }
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  save() {
    if (!this.selectedFile && !this.editingItem) {
      this.notification.error('Please upload an image file.', 'Validation');
      return;
    }

    const formData = new FormData();
    formData.append('title', this.model.title || '');
    formData.append('subtitle', this.model.subtitle || '');
    formData.append('linkUrl', this.model.linkUrl || '');
    formData.append('displayOrder', String(this.model.displayOrder));
    formData.append('isActive', String(this.model.isActive));

    if (this.selectedFile) {
      formData.append('ImageFile', this.selectedFile);
    }

    const editId = this.editingItem ? this.editingItem.sliderImageID : null;
    const request = editId 
      ? this.svc.update(editId, formData) 
      : this.svc.create(formData);

    request.subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Slider image saved successfully');
          this.closeModal();
          this.load();
        } else {
          this.notification.error(res.message || 'Failed to save');
        }
      },
      error: () => {
        this.notification.error('An error occurred while saving.');
      }
    });
  }

  deleteItem(id: number) {
    if (confirm('Are you sure you want to delete this slider image?')) {
      this.svc.delete(id).subscribe({
        next: (res: any) => {
          if (res.success) {
            this.notification.success('Slider image deleted successfully');
            this.load();
          } else {
            this.notification.error(res.message || 'Failed to delete');
          }
        },
        error: () => {
          this.notification.error('An error occurred while deleting.');
        }
      });
    }
  }
}
