import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { AuthService } from '../../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';

interface LeaderboardCustomer {
  userId: number;
  name: string;
  email: string;
  phone: string;
  picture: string | null;
  totalTickets: number;
  totalSpent: number;
  totalLogins: number;
  totalTimeSpentMinutes: number;
}

@Component({
  selector: 'app-admin-leaderboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './admin-leaderboard.component.html',
  styleUrls: ['./admin-leaderboard.component.css']
})
export class AdminLeaderboardComponent implements OnInit {
  customers: LeaderboardCustomer[] = [];
  isLoading = false;
  
  timeframe = 'all';
  sortBy = 'tickets';
  companyId = ''; // Empty means all companies
  
  companies: any[] = [];
  isAdmin = false;

  constructor(private http: HttpClient, private authService: AuthService, private router: Router) {
    this.isAdmin = this.authService.isAdmin() || this.authService.isAgent();
  }

  navigateToUser(userId: number) {
    this.router.navigate(['/admin/users', userId]);
  }

  ngOnInit(): void {
    if (this.isAdmin) {
      this.fetchCompanies();
    }
    this.fetchLeaderboard();
  }

  fetchCompanies() {
    this.http.get<any>(`${environment.apiUrl}/companies`).subscribe({
      next: (res) => {
        if (res.success) {
          this.companies = res.data;
        }
      }
    });
  }

  fetchLeaderboard() {
    this.isLoading = true;
    let url = `${environment.apiUrl}/leaderboard/customers?timeframe=${this.timeframe}&sortBy=${this.sortBy}`;
    
    if (this.isAdmin && this.companyId) {
      url += `&companyId=${this.companyId}`;
    }

    this.http.get<any>(url).subscribe({
      next: (res) => {
        if (res.success) {
          this.customers = res.data;
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onFilterChange() {
    this.fetchLeaderboard();
  }

  getRankBadge(index: number): {icon: string, color: string, bg: string} {
    if (index === 0) return { icon: 'fas fa-trophy', color: 'text-yellow-600', bg: 'bg-yellow-100' };
    if (index === 1) return { icon: 'fas fa-medal', color: 'text-gray-500', bg: 'bg-gray-200' };
    if (index === 2) return { icon: 'fas fa-award', color: 'text-amber-700', bg: 'bg-amber-100' };
    return { icon: '', color: 'text-gray-400', bg: 'bg-gray-50' };
  }
}
