import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css']
})
export class ContactComponent {
  private http = inject(HttpClient);
  
  formData = {
    name: '',
    email: '',
    bookingReference: '',
    category: 'Payment Pending / Callback Issue',
    message: ''
  };

  isSubmitting = false;

  onSubmit() {
    if (!this.formData.name || !this.formData.email || !this.formData.message) {
      alert('Please fill out all required fields.');
      return;
    }

    this.isSubmitting = true;
    this.http.post(`${environment.apiUrl}/Chat/contact`, this.formData).subscribe({
      next: () => {
        alert('Thank you for contacting Saowari Support! Your message has been sent to our live support team. We will get back to you shortly.');
        this.formData = {
          name: '',
          email: '',
          bookingReference: '',
          category: 'Payment Pending / Callback Issue',
          message: ''
        };
        this.isSubmitting = false;
      },
      error: (err) => {
        console.error('Error submitting form', err);
        alert('Failed to send message. Please try again later.');
        this.isSubmitting = false;
      }
    });
  }
}
