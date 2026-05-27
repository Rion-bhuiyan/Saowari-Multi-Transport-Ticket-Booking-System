import { Component, OnInit } from '@angular/core';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../../../core/services/api/dashboard.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CompanyService } from '../../../../core/services/api/company.service';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './admin-reports.component.html',
  styleUrls: ['./admin-reports.component.css']
})
export class AdminReportsComponent implements OnInit {
  pRevenue: number = 1;
  pageSizeRevenue: number = 15;
  get pagedRevenueData() {
    const start = (this.pRevenue - 1) * Number(this.pageSizeRevenue);
    return (this.revenueData || []).slice(start, start + Number(this.pageSizeRevenue));
  }

  pOccupancy: number = 1;
  pageSizeOccupancy: number = 15;
  get pagedOccupancyData() {
    const start = (this.pOccupancy - 1) * Number(this.pageSizeOccupancy);
    return (this.occupancyData || []).slice(start, start + Number(this.pageSizeOccupancy));
  }

  revenueData: any[] = [];
  occupancyData: any[] = [];
  companies: any[] = [];
  selectedCompanyId = '';
  isLoadingRevenue = true;
  isLoadingOccupancy = true;

  revenueParams: any = {
    startDate: this.getDateOffset(-30),
    endDate: this.getDateOffset(0),
    groupBy: 'day'
  };

  constructor(
    private dashboardService: DashboardService,
    public authService: AuthService,
    private companyService: CompanyService
  ) {}

  ngOnInit(): void {
    if (this.authService.isAdmin()) {
      this.companyService.getAll().subscribe({
        next: (res: any) => {
          if (res.success) this.companies = res.data || [];
        }
      });
    }
    this.loadRevenue();
    this.loadOccupancy();
  }

  getDateOffset(days: number): string {
    const d = new Date();
    d.setDate(d.getDate() + days);
    return d.toISOString().split('T')[0];
  }

  onCompanyChange() {
    this.loadRevenue();
    this.loadOccupancy();
  }

  loadRevenue() {
    this.isLoadingRevenue = true;
    const params = { ...this.revenueParams };
    if (this.selectedCompanyId) {
      params.companyId = this.selectedCompanyId;
    }
    this.dashboardService.getRevenueReport(params).subscribe({
      next: (res: any) => {
        if (res.success) this.revenueData = res.data || [];
        this.isLoadingRevenue = false;
      },
      error: () => { this.isLoadingRevenue = false; }
    });
  }

  loadOccupancy() {
    this.isLoadingOccupancy = true;
    const params = { ...this.revenueParams };
    if (this.selectedCompanyId) {
      params.companyId = this.selectedCompanyId;
    }
    this.dashboardService.getOccupancyReport(params).subscribe({
      next: (res: any) => {
        if (res.success) this.occupancyData = res.data || [];
        this.isLoadingOccupancy = false;
      },
      error: () => { this.isLoadingOccupancy = false; }
    });
  }

  getTotalRevenue(): number {
    return this.revenueData.reduce((s: number, i: any) => s + (i.revenue || i.amount || 0), 0);
  }
}
