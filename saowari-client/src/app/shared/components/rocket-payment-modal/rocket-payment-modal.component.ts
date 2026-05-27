import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-rocket-payment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rocket-payment-modal.component.html',
  styleUrls: ['./rocket-payment-modal.component.css']
})
export class RocketPaymentModalComponent {
  @Input() amount: number = 0;
  @Input() invoiceNo: string = '';
  @Input() logoUrl?: string;
  @Output() paymentSuccess = new EventEmitter<string>();
  @Output() paymentCancel = new EventEmitter<void>();

  mobileAccount: string = '';
  pin: string = '';
  isLoading: boolean = false;

  onSubmit() {
    if (!this.mobileAccount || this.mobileAccount.length < 11) {
      return;
    }
    if (!this.pin || this.pin.length < 4) {
      return;
    }

    this.isLoading = true;
    // Simulate API call delay
    setTimeout(() => {
      const mockTransactionId = 'DBBL' + Math.random().toString(36).substr(2, 9).toUpperCase();
      this.paymentSuccess.emit(mockTransactionId);
    }, 1500);
  }

  onReset() {
    this.mobileAccount = '';
    this.pin = '';
  }

  onBack() {
    this.paymentCancel.emit();
  }
}
