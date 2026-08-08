import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';
import { TrafficAnalyticsService } from '../../../../core/services/api/traffic-analytics.service';
import { ChartData, ChartOptions } from 'chart.js';

@Component({
  selector: 'app-admin-traffic-analytics',
  standalone: true,
  imports: [CommonModule, NgChartsModule],
  templateUrl: './admin-traffic-analytics.component.html',
  styleUrls: ['./admin-traffic-analytics.component.css']
})
export class AdminTrafficAnalyticsComponent implements OnInit {
  isLoading = true;
  data: any = null;

  deviceChartData: ChartData<'doughnut'> | null = null;
  channelChartData: ChartData<'bar'> | null = null;
  socialChartData: ChartData<'bar'> | null = null;
  browserChartData: ChartData<'doughnut'> | null = null;

  deviceChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: { legend: { display: false }, tooltip: { enabled: true } }
  };

  channelChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: {
      y: { beginAtZero: true, grid: { color: '#f1f5f9' }, ticks: { callback: (v: any) => v + '%', color: '#94a3b8', font: { size: 11 } } },
      x: { grid: { display: false }, ticks: { color: '#64748b', font: { size: 10 } } }
    }
  };

  socialChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: { legend: { display: false }, tooltip: { callbacks: { label: (ctx: any) => ctx.raw + '%' } } },
    scales: {
      x: { beginAtZero: true, max: 100, display: false },
      y: { grid: { display: false }, ticks: { color: '#475569', font: { size: 12 } } }
    }
  };

  browserChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '60%',
    plugins: { legend: { display: false }, tooltip: { enabled: true } }
  };

  readonly SOCIAL_COLORS: Record<string, string> = {
    'YouTube': '#FF0000',
    'Facebook': '#1877F2',
    'Instagram': '#E4405F',
    'X (Twitter)': '#000000',
    'LinkedIn': '#0A66C2',
    'Pinterest': '#E60023',
    'Telegram': '#26A5E4',
    'TikTok': '#69C9D0'
  };

  readonly BROWSER_COLORS: string[] = ['#3b82f6','#f97316','#10b981','#8b5cf6','#ef4444','#94a3b8'];

  constructor(private analyticsService: TrafficAnalyticsService) {}

  ngOnInit(): void {
    this.analyticsService.getAnalytics().subscribe({
      next: (res: any) => {
        if (res.success) {
          this.data = res.data;
          this.buildCharts();
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  buildCharts() {
    const d = this.data;

    // Device chart
    this.deviceChartData = {
      labels: ['Desktop', 'Mobile'],
      datasets: [{
        data: [d.deviceDistribution.desktop, d.deviceDistribution.mobile],
        backgroundColor: ['#3b82f6', '#93c5fd'],
        hoverBackgroundColor: ['#2563eb', '#60a5fa'],
        borderWidth: 0
      }]
    };

    // Channel chart
    const channels = d.marketingChannels || [];
    this.channelChartData = {
      labels: channels.map((c: any) => c.channel),
      datasets: [{
        data: channels.map((c: any) => c.percentage),
        backgroundColor: channels.map((_: any, i: number) =>
          ['#3b82f6','#60a5fa','#93c5fd','#1d4ed8','#2563eb','#6366f1','#8b5cf6','#a78bfa','#c4b5fd','#ddd6fe'][i] || '#3b82f6'),
        borderRadius: 4,
        barPercentage: 0.5
      }]
    };

    // Social chart
    const social = d.socialTraffic || [];
    this.socialChartData = {
      labels: social.map((s: any) => s.network),
      datasets: [{
        data: social.map((s: any) => s.percentage),
        backgroundColor: social.map((s: any) => this.SOCIAL_COLORS[s.network] || '#94a3b8'),
        borderRadius: 4,
        barPercentage: 0.5
      }]
    };

    // Browser chart
    const browsers = d.browserStats || [];
    this.browserChartData = {
      labels: browsers.map((b: any) => b.browser),
      datasets: [{
        data: browsers.map((b: any) => b.percentage),
        backgroundColor: this.BROWSER_COLORS.slice(0, browsers.length),
        borderWidth: 0
      }]
    };
  }

  hasSocialData(): boolean {
    return this.data?.socialTraffic?.length > 0 &&
      !(this.data.socialTraffic.length === 1 && this.data.socialTraffic[0].network === 'No social traffic yet');
  }

  getSocialColor(network: string): string {
    return this.SOCIAL_COLORS[network] || '#94a3b8';
  }
}
