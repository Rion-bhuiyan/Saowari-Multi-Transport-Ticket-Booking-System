import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';

export interface SeatClassModel {
  seatClassId: number;
  seatClassName: string;
}

@Injectable({ providedIn: 'root' })
export class SeatClassService {
  private apiUrl = `${environment.apiUrl}/seatclasss`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<SeatClassModel[]>> {
    return this.http.get<ApiResponse<SeatClassModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<SeatClassModel>> {
    return this.http.get<ApiResponse<SeatClassModel>>(`${this.apiUrl}/${id}`);
  }

  create(data: Partial<SeatClassModel>): Observable<ApiResponse<SeatClassModel>> {
    return this.http.post<ApiResponse<SeatClassModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<SeatClassModel>): Observable<ApiResponse<SeatClassModel>> {
    return this.http.put<ApiResponse<SeatClassModel>>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
