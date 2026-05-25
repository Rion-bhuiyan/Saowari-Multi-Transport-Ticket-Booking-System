import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { ScheduleModel } from '../../models/master.model';
import { TripSearchResult } from '../../models/business.model';

@Injectable({ providedIn: 'root' })
export class ScheduleService {
  private apiUrl = `${environment.apiUrl}/schedules`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<ScheduleModel[]>> {
    return this.http.get<ApiResponse<ScheduleModel[]>>(this.apiUrl);
  }

  getUpcoming(): Observable<ApiResponse<TripSearchResult[]>> {
    return this.http.get<ApiResponse<TripSearchResult[]>>(`${this.apiUrl}/upcoming`);
  }

  getById(id: number): Observable<ApiResponse<ScheduleModel>> {
    return this.http.get<ApiResponse<ScheduleModel>>(`${this.apiUrl}/${id}`);
  }

  getSeatMap(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/seat-map`);
  }

  available(params: any): Observable<ApiResponse<ScheduleModel[]>> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== null && params[key] !== undefined) {
        httpParams = httpParams.append(key, params[key]);
      }
    });
    return this.http.get<ApiResponse<ScheduleModel[]>>(`${this.apiUrl}/available`, { params: httpParams });
  }

  byRoute(routeId: number): Observable<ApiResponse<ScheduleModel[]>> {
    return this.http.get<ApiResponse<ScheduleModel[]>>(`${this.apiUrl}/by-route/${routeId}`);
  }

  byVehicle(vehicleId: number): Observable<ApiResponse<ScheduleModel[]>> {
    return this.http.get<ApiResponse<ScheduleModel[]>>(`${this.apiUrl}/by-vehicle/${vehicleId}`);
  }

  byDateRange(start: string, end: string): Observable<ApiResponse<ScheduleModel[]>> {
    return this.http.get<ApiResponse<ScheduleModel[]>>(`${this.apiUrl}/by-date?startDate=${start}&endDate=${end}`);
  }

  create(data: Partial<ScheduleModel>): Observable<ApiResponse<ScheduleModel>> {
    return this.http.post<ApiResponse<ScheduleModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<ScheduleModel>): Observable<ApiResponse<ScheduleModel>> {
    return this.http.put<ApiResponse<ScheduleModel>>(`${this.apiUrl}/${id}`, data);
  }

  patchStatus(id: number, statusId: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, statusId);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  // ── Lifecycle API ───────────────────────────────────────────────────────────

  getLifecycle(companyId?: number): Observable<any> {
    let params = new HttpParams();
    if (companyId) params = params.set('companyId', companyId.toString());
    return this.http.get<any>(`${this.apiUrl}/lifecycle`, { params });
  }

  markPendingExpiry(id: number): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/${id}/mark-pending`, {});
  }

  approveExpiry(id: number): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/${id}/approve-expiry`, {});
  }

  cloneSchedule(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/clone`, payload);
  }
}
