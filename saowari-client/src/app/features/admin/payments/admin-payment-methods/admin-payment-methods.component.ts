import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentMethodService, PaymentMethodModel } from '../../../../core/services/api/payment-method.service';
import { NotificationService } from '../../../../core/services/notification.service';

interface FormState {
  paymentMethodName: string;
  processingFeePercent: number;
  vatPercent: number;
  isActive: boolean;
}

@Component({
  selector: 'app-admin-payment-methods',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-payment-methods.component.html',
  styleUrls: ['./admin-payment-methods.component.css']
})
export class AdminPaymentMethodsComponent implements OnInit {
  methods: PaymentMethodModel[] = [];
  isLoading = false;
  isSaving = false;

  showForm = false;
  showDeleteConfirm = false;
  editingId: number | null = null;
  deletingMethod: PaymentMethodModel | null = null;
  deleteConfirmName = '';

  logoFile: File | null = null;
  logoPreview: string | null = null;

  form: FormState = this.defaultForm();

  constructor(
    private pmService: PaymentMethodService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void { this.loadAll(); }

  loadAll() {
    this.isLoading = true;
    this.pmService.getAll().subscribe({
      next: res => {
        this.methods = res.data || [];
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  openForm(method?: PaymentMethodModel) {
    if (method) {
      this.editingId = method.paymentMethodId;
      this.form = {
        paymentMethodName: method.paymentMethodName,
        processingFeePercent: method.processingFeePercent,
        vatPercent: method.vatPercent,
        isActive: method.isActive
      };
      this.logoPreview = method.logoUrl || null;
    } else {
      this.editingId = null;
      this.form = this.defaultForm();
      this.logoPreview = null;
    }
    this.logoFile = null;
    this.showForm = true;
  }

  closeForm() {
    this.showForm = false;
    this.logoFile = null;
    this.logoPreview = null;
  }

  onLogoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.logoFile = input.files[0];
      const reader = new FileReader();
      reader.onload = e => this.logoPreview = e.target?.result as string;
      reader.readAsDataURL(this.logoFile);
    }
  }

  saveMethod() {
    this.isSaving = true;

    const fd = new FormData();
    fd.append('paymentMethodName', this.form.paymentMethodName);
    fd.append('processingFeePercent', this.form.processingFeePercent.toString());
    fd.append('vatPercent', this.form.vatPercent.toString());
    fd.append('isActive', this.form.isActive.toString());
    if (this.logoFile) fd.append('logoFile', this.logoFile, this.logoFile.name);

    const obs = this.editingId
      ? this.pmService.update(this.editingId, fd)
      : this.pmService.create(fd);

    obs.subscribe({
      next: res => {
        if (res.success) {
          this.notification.success(
            this.editingId ? 'Payment method updated!' : 'Payment method created!',
            'Success'
          );
          this.closeForm();
          this.loadAll();
        } else {
          this.notification.error(res.message || 'Failed to save.', 'Error');
        }
        this.isSaving = false;
      },
      error: err => {
        this.notification.error(err?.error?.message || 'Server error.', 'Error');
        this.isSaving = false;
      }
    });
  }

  confirmDelete(method: PaymentMethodModel) {
    this.deletingMethod = method;
    this.deleteConfirmName = '';
    this.showDeleteConfirm = true;
  }

  deleteMethod() {
    if (!this.deletingMethod) return;
    this.isSaving = true;
    this.pmService.delete(this.deletingMethod.paymentMethodId).subscribe({
      next: res => {
        if (res.success) {
          this.notification.success('Payment method deleted.', 'Deleted');
          this.loadAll();
        } else {
          this.notification.error(res.message || 'Failed to delete.', 'Error');
        }
        this.showDeleteConfirm = false;
        this.isSaving = false;
      },
      error: () => {
        this.notification.error('Server error.', 'Error');
        this.isSaving = false;
      }
    });
  }

  getEffectiveRate(m: PaymentMethodModel): number {
    const fee = m.processingFeePercent;
    const vat = fee * m.vatPercent / 100;
    return fee + vat;
  }

  getPreviewTotal(): number {
    const base = 1000;
    const fee = base * this.form.processingFeePercent / 100;
    const vat = fee * this.form.vatPercent / 100;
    return base + fee + vat;
  }

  private defaultForm(): FormState {
    return { paymentMethodName: '', processingFeePercent: 0, vatPercent: 0, isActive: true };
  }
}
