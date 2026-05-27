import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-broadcast',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './broadcast.component.html',
  styleUrls: ['./broadcast.component.css']
})
export class BroadcastComponent implements OnInit {
  subject: string = '';
  message: string = '';
  selectedImage: File | null = null;
  imagePreview: string | null = null;
  
  roles: any[] = [];
  selectedRoleIds: number[] = [];

  isSubmitting: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.fetchRoles();
  }

  fetchRoles() {
    this.http.get<any>(`${environment.apiUrl}/userroles`).subscribe({
      next: (res) => {
        if (res.success) {
          this.roles = res.data;
        }
      },
      error: (err) => console.error('Failed to load roles', err)
    });
  }

  toggleRole(roleId: number, event: any) {
    if (event.target.checked) {
      this.selectedRoleIds.push(roleId);
    } else {
      this.selectedRoleIds = this.selectedRoleIds.filter(id => id !== roleId);
    }
  }

  onImageSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedImage = file;
      
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage() {
    this.selectedImage = null;
    this.imagePreview = null;
  }

  onSubmit() {
    if (!this.subject.trim() || !this.message.trim()) {
      this.errorMessage = 'Subject and Message are required.';
      return;
    }

    if (this.selectedRoleIds.length === 0) {
      this.errorMessage = 'Please select at least one role to broadcast to.';
      return;
    }

    this.isSubmitting = true;
    this.successMessage = '';
    this.errorMessage = '';

    const formData = new FormData();
    formData.append('Subject', this.subject);
    formData.append('Message', this.message);
    
    this.selectedRoleIds.forEach(id => {
      formData.append('TargetRoleIds', id.toString());
    });

    if (this.selectedImage) {
      formData.append('Image', this.selectedImage);
    }

    this.http.post<any>(`${environment.apiUrl}/notifications/broadcast`, formData).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.success) {
          this.successMessage = res.message || 'Broadcast sent successfully!';
          this.resetForm();
        } else {
          this.errorMessage = res.message || 'Failed to send broadcast.';
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'An error occurred while sending the broadcast.';
      }
    });
  }

  resetForm() {
    this.subject = '';
    this.message = '';
    this.selectedImage = null;
    this.imagePreview = null;
    this.selectedRoleIds = [];
    
    // Reset checkboxes visually if needed, though with angular form bindings it's usually better to just re-fetch or clear array
    // Since we don't bind checkboxes to model directly (using toggle), we need to clear selectedRoleIds which we did.
    // The HTML will handle unchecked state if we bind [checked]="selectedRoleIds.includes(role.userRoleId)".
  }
}
