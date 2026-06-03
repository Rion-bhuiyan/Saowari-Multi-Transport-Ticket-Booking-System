import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { CompanyService } from '../../../../core/services/api/company.service';

@Component({
  selector: 'app-company-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './company-profile.component.html',
  styleUrls: ['./company-profile.component.css']
})
export class CompanyProfileComponent implements OnInit {
  companyId: number | null = null;
  company: any = null;
  isLoading = true;
  isSaving = false;

  model: any = {
    companyName: '',
    contactEmail: '',
    contactPhone: '',
    address: '',
    ticketBackgroundOpacity: 0.1
  };

  logoFile: File | null = null;
  logoPreview: string | null = null;

  ticketBgFile: File | null = null;
  ticketBgPreview: string | null = null;

  constructor(
    private authService: AuthService,
    private companyService: CompanyService
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (user && user.companyId) {
      this.companyId = user.companyId;
      this.loadCompanyDetails();
    } else {
      this.isLoading = false;
      alert('Company Information not found for your account.');
    }
  }

  loadCompanyDetails(): void {
    if (!this.companyId) return;
    this.isLoading = true;
    this.companyService.getById(this.companyId).subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.company = res.data;
          this.model = {
            companyName: this.company.companyName,
            contactEmail: this.company.contactEmail,
            contactPhone: this.company.contactPhone,
            address: this.company.address,
            ticketBackgroundOpacity: this.company.ticketBackgroundOpacity != null ? this.company.ticketBackgroundOpacity : 0.1
          };
          this.logoPreview = this.company.logoURL;
          this.ticketBgPreview = this.company.ticketBackgroundUrl;
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Failed to load company details', err);
        alert('Failed to load company profile');
      }
    });
  }

  onLogoChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.logoFile = file;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.logoPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  onTicketBgChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.ticketBgFile = file;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.ticketBgPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  saveChanges(): void {
    if (!this.companyId) return;

    this.isSaving = true;
    const formData = new FormData();
    formData.append('companyName', this.model.companyName);
    formData.append('contactEmail', this.model.contactEmail);
    formData.append('contactPhone', this.model.contactPhone);
    formData.append('address', this.model.address || '');
    formData.append('companyTypeId', this.company.companyTypeId); // Preserve type
    formData.append('isActive', 'true');
    formData.append('ticketBackgroundOpacity', (this.model.ticketBackgroundOpacity || 0.1).toString());

    if (this.logoFile) {
      formData.append('logoFile', this.logoFile);
    }
    if (this.ticketBgFile) {
      formData.append('ticketBackgroundImage', this.ticketBgFile);
    }

    this.companyService.update(this.companyId, formData).subscribe({
      next: (res: any) => {
        this.isSaving = false;
        if (res.success) {
          alert('Company profile updated successfully');
          this.loadCompanyDetails(); // Reload fresh data
        } else {
          alert(res.message || 'Failed to update');
        }
      },
      error: (err) => {
        this.isSaving = false;
        console.error(err);
        alert('An error occurred while saving');
      }
    });
  }
}
