import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/api/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UserModel } from '../../../core/models/auth.model';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './edit-profile.component.html',
  styleUrls: ['./edit-profile.component.css']
})
export class EditProfileComponent implements OnInit {
  model: Partial<UserModel> = {};
  isLoading = false;
  isSaving = false;

  selectedFile: File | null = null;

  expandedPictureUrl: string | null = null;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.model = { ...user };
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      
      // For immediate preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.model.picture = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  onSave() {
    this.isSaving = true;

    const formData = new FormData();
    if (this.model.fullName) formData.append('FullName', this.model.fullName);
    if (this.model.phone) formData.append('Phone', this.model.phone);
    if (this.selectedFile) formData.append('PictureFile', this.selectedFile);
    // Keep picture string if no new file is uploaded so we don't accidentally clear it if the backend handles it that way.
    if (this.model.picture && !this.selectedFile) formData.append('Picture', this.model.picture);

    this.userService.updateProfile(formData).subscribe({
      next: (res: any) => {
        this.isSaving = false;
        if (res.success) {
          // Update the locally cached auth user so the nav updates
          const current = this.authService.currentUserValue;
          if (current) {
            this.authService.updateCurrentUser({ ...current, ...res.data });
          }
          this.notification.success('Profile updated successfully.', 'Success');
        } else {
          this.notification.error(res.message || 'Update failed.', 'Error');
        }
      },
      error: () => {
        this.isSaving = false;
        this.notification.error('An error occurred.', 'Error');
      }
    });
  }

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

  getInitials(): string {
    if (!this.model.fullName) return 'U';
    return this.model.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
