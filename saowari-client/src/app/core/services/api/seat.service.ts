import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { SeatModel } from '../../models/master.model';

@Injectable({ providedIn: 'root' })
export class SeatService {
  private apiUrl = `${environment.apiUrl}/seats`;

  constructor(private http: HttpClient) {}

  getByVehicle(vehicleId: number): Observable<ApiResponse<SeatModel[]>> {
    return this.http.get<ApiResponse<SeatModel[]>>(`${this.apiUrl}/by-vehicle/${vehicleId}`);
  }

  getById(id: number): Observable<ApiResponse<SeatModel>> {
    return this.http.get<ApiResponse<SeatModel>>(`${this.apiUrl}/${id}`);
  }

  create(data: Partial<SeatModel>): Observable<ApiResponse<SeatModel>> {
    return this.http.post<ApiResponse<SeatModel>>(this.apiUrl, data);
  }

  bulkCreate(data: any): Observable<ApiResponse<SeatModel[]>> {
    return this.http.post<ApiResponse<SeatModel[]>>(`${this.apiUrl}/bulk`, data);
  }

  update(id: number, data: Partial<SeatModel>): Observable<ApiResponse<SeatModel>> {
    return this.http.put<ApiResponse<SeatModel>>(`${this.apiUrl}/${id}`, data);
  }

  patchActive(id: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, isActive);
  }
}
