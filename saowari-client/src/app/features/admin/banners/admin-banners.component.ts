import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-admin-banners',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-banners.component.html'
})
export class AdminBannersComponent implements OnInit {
  banners: any[] = [];
  isLoading = false;
  
  // Modal state
  isModalOpen = false;
  isEditing = false;
  currentBannerId: number | null = null;
  
  // Form state
  bannerForm = {
    title: '',
    linkUrl: '',
    position: 'UpcomingTrips',
    isActive: true
  };
  
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadBanners();
  }

  loadBanners() {
    this.isLoading = true;
    this.http.get<any>(`${environment.apiUrl}/banners/all`).subscribe({
      next: (res) => {
        if (res.success) {
          this.banners = res.data;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading banners', err);
        this.isLoading = false;
      }
    });
  }

  openAddModal() {
    this.isEditing = false;
    this.currentBannerId = null;
    this.bannerForm = { title: '', linkUrl: '', position: 'UpcomingTrips', isActive: true };
    this.selectedFile = null;
    this.imagePreview = null;
    this.isModalOpen = true;
  }

  openEditModal(banner: any) {
    this.isEditing = true;
    this.currentBannerId = banner.bannerId;
    this.bannerForm = {
      title: banner.title || '',
      linkUrl: banner.linkUrl || '',
      position: banner.position,
      isActive: banner.isActive
    };
    this.selectedFile = null;
    this.imagePreview = banner.imageUrl;
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
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  saveBanner() {
    if (!this.isEditing && !this.selectedFile) {
      alert('Please select an image for the banner.');
      return;
    }

    const formData = new FormData();
    if (this.bannerForm.title) formData.append('Title', this.bannerForm.title);
    if (this.bannerForm.linkUrl) formData.append('LinkUrl', this.bannerForm.linkUrl);
    formData.append('Position', this.bannerForm.position);
    formData.append('IsActive', String(this.bannerForm.isActive));
    
    if (this.selectedFile) {
      formData.append('Image', this.selectedFile);
    }

    if (this.isEditing && this.currentBannerId) {
      this.http.put<any>(`${environment.apiUrl}/banners/${this.currentBannerId}`, formData).subscribe({
        next: (res) => {
          if (res.success) {
            this.loadBanners();
            this.closeModal();
          }
        },
        error: (err) => console.error('Error updating banner', err)
      });
    } else {
      this.http.post<any>(`${environment.apiUrl}/banners`, formData).subscribe({
        next: (res) => {
          if (res.success) {
            this.loadBanners();
            this.closeModal();
          }
        },
        error: (err) => console.error('Error creating banner', err)
      });
    }
  }

  deleteBanner(id: number) {
    if (confirm('Are you sure you want to delete this banner?')) {
      this.http.delete<any>(`${environment.apiUrl}/banners/${id}`).subscribe({
        next: (res) => {
          if (res.success) {
            this.loadBanners();
          }
        },
        error: (err) => console.error('Error deleting banner', err)
      });
    }
  }
}
