import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../../core/services/api/settings.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-settings.component.html'
})
export class AdminSettingsComponent implements OnInit {
  // ── Logo ──────────────────────────────────────────────
  currentLogoUrl: string | null = null;
  logoPreview: string | ArrayBuffer | null = null;
  selectedLogoFile: File | null = null;
  isLogoLoading = false;

  // ── Ticket Background ─────────────────────────────────
  currentBgUrl: string | null = null;
  bgPreview: string | ArrayBuffer | null = null;
  selectedBgFile: File | null = null;
  isBgLoading = false;
  ticketBgOpacity = 0.1;

  // ── Global Site Appearance ────────────────────────────
  enableBackgroundPattern = false;
  isAppearanceLoading = false;

  constructor(
    private settingsService: SettingsService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadLogo();
    this.loadTicketBackground();
    this.loadSystemSettings();
  }

  loadSystemSettings() {
    this.settingsService.getSystemSettings().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          const opacityStr = res.data.TicketBackgroundOpacity || res.data.ticketBackgroundOpacity;
          if (opacityStr) {
            this.ticketBgOpacity = parseFloat(opacityStr);
          }
          
          const patternStr = res.data.EnableBackgroundPattern || res.data.enableBackgroundPattern;
          if (patternStr) {
            this.enableBackgroundPattern = patternStr.toLowerCase() === 'true';
          }
        }
      }
    });
  }

  toggleBackgroundPattern() {
    this.isAppearanceLoading = true;
    const val = this.enableBackgroundPattern ? 'true' : 'false';
    this.settingsService.updateSystemSettings({ EnableBackgroundPattern: val }).subscribe({
      next: (res: any) => {
        this.isAppearanceLoading = false;
        if (res.success) {
          this.notification.success('Background pattern setting updated');
        } else {
          this.notification.error('Failed to update background setting');
          // revert if failed
          this.enableBackgroundPattern = !this.enableBackgroundPattern;
        }
      },
      error: () => {
        this.isAppearanceLoading = false;
        this.notification.error('An error occurred');
        this.enableBackgroundPattern = !this.enableBackgroundPattern;
      }
    });
  }

  saveOpacity() {
    this.settingsService.updateSystemSettings({ TicketBackgroundOpacity: this.ticketBgOpacity.toString() }).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.notification.success('Global opacity updated successfully');
        } else {
          this.notification.error('Failed to update opacity');
        }
      }
    });
  }

  // ── Logo Methods ──────────────────────────────────────
  loadLogo() {
    this.settingsService.getLogo().subscribe({
      next: (res: any) => {
        if (res.success && res.data) this.currentLogoUrl = res.data;
      }
    });
  }

  onLogoSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedLogoFile = file;
      const reader = new FileReader();
      reader.onload = () => this.logoPreview = reader.result;
      reader.readAsDataURL(file);
    }
  }

  removeLogoSelection() {
    this.selectedLogoFile = null;
    this.logoPreview = null;
  }

  saveLogo() {
    if (!this.selectedLogoFile) return;
    this.isLogoLoading = true;
    const formData = new FormData();
    formData.append('logoFile', this.selectedLogoFile);
    this.settingsService.uploadLogo(formData).subscribe({
      next: (res: any) => {
        this.isLogoLoading = false;
        if (res.success) {
          this.notification.success('Logo updated successfully');
          this.currentLogoUrl = res.data;
          this.removeLogoSelection();
          window.location.reload();
        } else {
          this.notification.error(res.message || 'Failed to upload logo');
        }
      },
      error: () => {
        this.isLogoLoading = false;
        this.notification.error('An error occurred while uploading');
      }
    });
  }

  // ── Ticket Background Methods ─────────────────────────
  loadTicketBackground() {
    this.settingsService.getTicketBackground().subscribe({
      next: (res: any) => {
        if (res.success && res.data) this.currentBgUrl = res.data;
      }
    });
  }

  onBgSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        const img = new Image();
        img.onload = () => {
          const width = img.width;
          const height = img.height;
          
          // Letter size portrait aspect ratio is 8.5 / 11 ≈ 0.7727
          // We allow a tolerance range of 0.7 to 0.85
          const ratio = width / height;
          
          if (ratio < 0.7 || ratio > 0.85) {
            this.notification.error('Please upload a Letter size portrait image (approx 8.5:11 aspect ratio). Landscape images will be distorted.');
            event.target.value = ''; // Reset input
            return;
          }
          
          this.selectedBgFile = file;
          this.bgPreview = e.target.result;
        };
        img.src = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  removeBgSelection() {
    this.selectedBgFile = null;
    this.bgPreview = null;
  }

  saveTicketBackground() {
    if (!this.selectedBgFile) return;
    this.isBgLoading = true;
    const formData = new FormData();
    formData.append('backgroundFile', this.selectedBgFile);
    this.settingsService.uploadTicketBackground(formData).subscribe({
      next: (res: any) => {
        this.isBgLoading = false;
        if (res.success) {
          this.notification.success('Ticket background updated successfully!');
          this.currentBgUrl = res.data;
          this.removeBgSelection();
        } else {
          this.notification.error(res.message || 'Failed to upload background');
        }
      },
      error: () => {
        this.isBgLoading = false;
        this.notification.error('An error occurred while uploading');
      }
    });
  }

  removeTicketBackground() {
    if (!this.currentBgUrl) return;
    this.isBgLoading = true;
    this.settingsService.deleteTicketBackground().subscribe({
      next: (res: any) => {
        this.isBgLoading = false;
        if (res.success) {
          this.notification.success('Ticket background removed');
          this.currentBgUrl = null;
        }
      },
      error: () => {
        this.isBgLoading = false;
        this.notification.error('Failed to remove background');
      }
    });
  }
}
