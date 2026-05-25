import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { RouteModel } from '../../models/master.model';

@Injectable({ providedIn: 'root' })
export class RouteService {
  private apiUrl = `${environment.apiUrl}/routes`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<RouteModel[]>> {
    return this.http.get<ApiResponse<RouteModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<RouteModel>> {
    return this.http.get<ApiResponse<RouteModel>>(`${this.apiUrl}/${id}`);
  }

  search(from: number, to: number): Observable<ApiResponse<RouteModel[]>> {
    return this.http.get<ApiResponse<RouteModel[]>>(`${this.apiUrl}?fromLocationId=${from}&toLocationId=${to}`);
  }

  create(data: Partial<RouteModel>): Observable<ApiResponse<RouteModel>> {
    return this.http.post<ApiResponse<RouteModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<RouteModel>): Observable<ApiResponse<RouteModel>> {
    return this.http.put<ApiResponse<RouteModel>>(`${this.apiUrl}/${id}`, data);
  }

  patchStatus(id: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, isActive);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
