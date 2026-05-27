import { Component, EventEmitter, Input, Output, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-bkash-payment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bkash-payment-modal.component.html',
  styleUrls: ['./bkash-payment-modal.component.css']
})
export class BkashPaymentModalComponent implements OnInit, OnDestroy {
  @Input() amount: number = 0;
  @Input() logoUrl?: string;
  @Output() paymentSuccess = new EventEmitter<string>();
  @Output() paymentCancel = new EventEmitter<void>();

  currentStep: 1 | 2 | 3 | 'cancel' = 1;
  previousStep: 1 | 2 | 3 = 1;

  accountNumber: string = '';
  verificationCode: string = '';
  pin: string = '';

  isLoading: boolean = false;
  
  resendCountdown: number = 30;
  private countdownInterval: any;

  ngOnInit() {
    this.currentStep = 1;
  }

  ngOnDestroy() {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }

  startCountdown() {
    this.resendCountdown = 30;
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
    this.countdownInterval = setInterval(() => {
      this.resendCountdown--;
      if (this.resendCountdown <= 0) {
        clearInterval(this.countdownInterval);
      }
    }, 1000);
  }

  resendCode() {
    if (this.resendCountdown <= 0) {
      // Simulate resending
      this.startCountdown();
      this.verificationCode = '';
      setTimeout(() => {
        if (this.currentStep === 2) {
          this.verificationCode = '123456';
        }
      }, 3000);
    }
  }

  onConfirm() {
    if (this.currentStep === 1) {
      if (!this.accountNumber || this.accountNumber.length < 11) {
        return;
      }
      this.currentStep = 2;
      this.previousStep = 2;
      // Auto-fill OTP after 3 seconds
      setTimeout(() => {
        if (this.currentStep === 2) {
          this.verificationCode = '123456';
        }
      }, 3000);
      
      this.startCountdown();
      
    } else if (this.currentStep === 2) {
      if (!this.verificationCode || this.verificationCode.length !== 6) {
        return;
      }
      this.currentStep = 3;
      this.previousStep = 3;
    } else if (this.currentStep === 3) {
      if (!this.pin || this.pin.length < 4) {
        return;
      }
      this.isLoading = true;
      setTimeout(() => {
        const mockTransactionId = 'TRN' + Math.random().toString(36).substr(2, 9).toUpperCase();
        this.paymentSuccess.emit(mockTransactionId);
      }, 1500);
    }
  }

  onCancel() {
    if (this.currentStep === 'cancel') {
      this.paymentCancel.emit();
    } else {
      this.previousStep = this.currentStep as 1 | 2 | 3;
      this.currentStep = 'cancel';
    }
  }

  onNoCancel() {
    this.currentStep = this.previousStep;
  }

  onYesCancel() {
    this.paymentCancel.emit();
  }

  getMaskedAccount(): string {
    if (this.accountNumber.length >= 11) {
      return this.accountNumber.substring(0, 3) + ' ** *** ' + this.accountNumber.substring(8);
    }
    return this.accountNumber;
  }
}
