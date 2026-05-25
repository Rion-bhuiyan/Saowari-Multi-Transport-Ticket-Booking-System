import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TicketService } from '../../../core/services/api/ticket.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-tickets.component.html',
  styleUrls: ['./my-tickets.component.css']
})
export class MyTicketsComponent implements OnInit {
  tickets: any[] = [];
  isLoading = true;

  constructor(
    private ticketService: TicketService,
    private authService: AuthService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets() {
    this.isLoading = true;
    this.ticketService.getByUser(0).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.tickets = res.data || [];
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  printTicket(ticket: any) {
    window.print();
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  formatTime(d: string): string {
    return new Date(d).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
}
