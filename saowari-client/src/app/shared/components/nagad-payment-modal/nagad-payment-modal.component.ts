import { Component, EventEmitter, Input, Output, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-nagad-payment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './nagad-payment-modal.component.html',
  styleUrls: ['./nagad-payment-modal.component.css']
})
export class NagadPaymentModalComponent implements OnInit, OnDestroy {
  @Input() amount: number = 0;
  @Input() logoUrl?: string;
  @Input() invoiceNo: string = '';
  @Output() paymentSuccess = new EventEmitter<string>();
  @Output() paymentCancel = new EventEmitter<void>();

  currentStep: 1 | 2 | 3 | 'cancel' = 1;
  previousStep: 1 | 2 | 3 = 1;

  accountNumber: string = '';
  verificationCode: string = '';
  pin: string = '';

  lang: 'en' | 'bn' = 'en';

  accountDigits: string[] = Array(11).fill('');
  pinDigits: string[] = Array(4).fill('');

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
        const mockTransactionId = 'NAGAD' + Math.random().toString(36).substr(2, 9).toUpperCase();
        this.paymentSuccess.emit(mockTransactionId);
      }, 1500);
    }
  }

  onCancel() {
    this.paymentCancel.emit();
  }

  toggleLang(newLang: 'en' | 'bn') {
    this.lang = newLang;
  }

  onDigitInput(event: any, index: number, type: 'account' | 'pin') {
    const value = event.target.value;
    if (value.length > 1) {
      event.target.value = value.substring(0, 1);
    }
    const arr = type === 'account' ? this.accountDigits : this.pinDigits;
    arr[index] = event.target.value;

    if (event.target.value) {
      const nextId = `${type}-${index + 1}`;
      const nextEl = document.getElementById(nextId);
      if (nextEl) nextEl.focus();
    }
    this.updateValues();
  }

  onDigitKeyDown(event: any, index: number, type: 'account' | 'pin') {
    if (event.key === 'Backspace') {
      const arr = type === 'account' ? this.accountDigits : this.pinDigits;
      if (!arr[index]) {
        const prevId = `${type}-${index - 1}`;
        const prevEl = document.getElementById(prevId);
        if (prevEl) {
          prevEl.focus();
          // Optionally clear the previous one
          // arr[index - 1] = ''; 
          // (event.target as HTMLInputElement).value = '';
        }
      } else {
        arr[index] = '';
      }
      this.updateValues();
    }
  }

  updateValues() {
    this.accountNumber = this.accountDigits.join('');
    this.pin = this.pinDigits.join('');
  }

  trackByIndex(index: number): number {
    return index;
  }
}
