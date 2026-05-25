import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { VehicleModel } from '../../models/master.model';

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private apiUrl = `${environment.apiUrl}/vehicles`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<VehicleModel[]>> {
    return this.http.get<ApiResponse<VehicleModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<VehicleModel>> {
    return this.http.get<ApiResponse<VehicleModel>>(`${this.apiUrl}/${id}`);
  }

  getByCompany(companyId: number): Observable<ApiResponse<VehicleModel[]>> {
    return this.http.get<ApiResponse<VehicleModel[]>>(`${this.apiUrl}/by-company/${companyId}`);
  }

  create(data: Partial<VehicleModel>): Observable<ApiResponse<VehicleModel>> {
    return this.http.post<ApiResponse<VehicleModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<VehicleModel>): Observable<ApiResponse<VehicleModel>> {
    return this.http.put<ApiResponse<VehicleModel>>(`${this.apiUrl}/${id}`, data);
  }

  patchActive(id: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, isActive);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  generateSeats(id: number, config: any): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/${id}/generate-seats`, config);
  }

  updateSeatClasses(id: number, assignments: any[]): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/${id}/seats/classes`, assignments);
  }
}
