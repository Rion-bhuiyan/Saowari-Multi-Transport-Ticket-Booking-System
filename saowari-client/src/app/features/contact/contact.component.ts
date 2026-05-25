import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css']
})
export class ContactComponent {
  onSubmit(event: Event) {
    event.preventDefault();
    alert('Thank you for contacting Saowari Support! We have received your query and our support team will get back to you shortly.');
    (event.target as HTMLFormElement).reset();
  }
}
