import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { DashboardSummary } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getSummary(): Observable<ApiResponse<DashboardSummary>> {
    return this.http.get<ApiResponse<DashboardSummary>>(`${this.apiUrl}/summary`);
  }

  getRevenueReport(params: any): Observable<ApiResponse<any>> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key]) httpParams = httpParams.append(key, params[key]);
    });
    return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/DashboardReports/revenue`, { params: httpParams });
  }

  getOccupancyReport(params: any): Observable<ApiResponse<any>> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key]) httpParams = httpParams.append(key, params[key]);
    });
    return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/DashboardReports/occupancy`, { params: httpParams });
  }
}
