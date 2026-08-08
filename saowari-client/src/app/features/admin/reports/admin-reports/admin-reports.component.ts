import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../../../core/services/api/dashboard.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CompanyService } from '../../../../core/services/api/company.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-reports.component.html',
  styleUrls: ['./admin-reports.component.css']
})
export class AdminReportsComponent implements OnInit, AfterViewInit {
  companies: any[] = [];
  selectedCompanyId = '';
  startDate: string = '';
  endDate: string = '';
  isLoading = true;

  analyticsData: any = null;

  @ViewChild('trendChart') trendChartRef!: ElementRef;
  @ViewChild('companyChart') companyChartRef!: ElementRef;

  trendChartInstance: Chart | null = null;
  companyChartInstance: Chart | null = null;

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
    this.loadAnalytics();
  }

  ngAfterViewInit() {
    // Charts will be initialized after data loads
  }

  onCompanyChange() {
    this.loadAnalytics();
  }

  loadAnalytics() {
    this.isLoading = true;
    const params: any = {};
    if (this.selectedCompanyId) {
      params.companyId = this.selectedCompanyId;
    }
    if (this.startDate) {
      params.startDate = this.startDate;
    }
    if (this.endDate) {
      params.endDate = this.endDate;
    }

    this.dashboardService.getAdvancedAnalytics(params).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.analyticsData = res.data;
          this.updateCharts();
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  updateCharts() {
    if (!this.analyticsData) return;

    // Wait a tick for canvas to render if it was hidden
    setTimeout(() => {
      this.renderTrendChart();
      this.renderCompanyChart();
    }, 100);
  }

  renderTrendChart() {
    if (!this.trendChartRef) return;
    const ctx = this.trendChartRef.nativeElement.getContext('2d');
    
    if (this.trendChartInstance) {
      this.trendChartInstance.destroy();
    }

    const labels = this.analyticsData.trend30Days.map((d: any) => {
      const date = new Date(d.date);
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    });
    const data = this.analyticsData.trend30Days.map((d: any) => d.revenue);

    // Gradient fill
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(99, 102, 241, 0.5)'); // Indigo
    gradient.addColorStop(1, 'rgba(99, 102, 241, 0.0)');

    this.trendChartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [{
          label: 'Revenue (৳)',
          data: data,
          borderColor: '#6366f1',
          backgroundColor: gradient,
          borderWidth: 3,
          pointBackgroundColor: '#fff',
          pointBorderColor: '#6366f1',
          pointBorderWidth: 2,
          pointRadius: 4,
          pointHoverRadius: 6,
          fill: true,
          tension: 0.4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: 'rgba(15, 23, 42, 0.9)',
            titleFont: { size: 13, family: 'Inter' },
            bodyFont: { size: 14, weight: 'bold', family: 'Inter' },
            padding: 12,
            cornerRadius: 8,
            displayColors: false
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { font: { family: 'Inter' }, color: '#94a3b8', maxTicksLimit: 10 }
          },
          y: {
            border: { display: false },
            grid: { color: 'rgba(148, 163, 184, 0.1)' },
            ticks: {
              font: { family: 'Inter' },
              color: '#94a3b8',
              callback: (value) => '৳' + value
            },
            beginAtZero: true
          }
        },
        interaction: { mode: 'nearest', axis: 'x', intersect: false }
      }
    });
  }

  renderCompanyChart() {
    if (!this.companyChartRef) return;
    const ctx = this.companyChartRef.nativeElement.getContext('2d');
    
    if (this.companyChartInstance) {
      this.companyChartInstance.destroy();
    }

    const labels = this.analyticsData.companyComparisons.map((c: any) => c.companyName);
    const data = this.analyticsData.companyComparisons.map((c: any) => c.revenue);

    this.companyChartInstance = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Revenue (৳)',
          data: data,
          backgroundColor: [
            'rgba(99, 102, 241, 0.9)',
            'rgba(168, 85, 247, 0.9)',
            'rgba(236, 72, 153, 0.9)',
            'rgba(245, 158, 11, 0.9)',
            'rgba(16, 185, 129, 0.9)'
          ],
          borderRadius: 8,
          borderSkipped: false,
          barPercentage: 0.6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: 'rgba(15, 23, 42, 0.9)',
            titleFont: { size: 13, family: 'Inter' },
            bodyFont: { size: 14, weight: 'bold', family: 'Inter' },
            padding: 12,
            cornerRadius: 8
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { font: { family: 'Inter' }, color: '#94a3b8' }
          },
          y: {
            border: { display: false },
            grid: { color: 'rgba(148, 163, 184, 0.1)' },
            ticks: {
              font: { family: 'Inter' },
              color: '#94a3b8',
              callback: (value) => '৳' + value
            },
            beginAtZero: true
          }
        }
      }
    });
  }
}
