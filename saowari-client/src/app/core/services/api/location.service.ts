import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { LocationModel } from '../../models/master.model';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private apiUrl = `${environment.apiUrl}/locations`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<LocationModel[]>> {
    return this.http.get<ApiResponse<LocationModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<LocationModel>> {
    return this.http.get<ApiResponse<LocationModel>>(`${this.apiUrl}/${id}`);
  }

  search(name: string): Observable<ApiResponse<LocationModel[]>> {
    return this.http.get<ApiResponse<LocationModel[]>>(`${this.apiUrl}?search=${name}`);
  }

  create(data: Partial<LocationModel>): Observable<ApiResponse<LocationModel>> {
    return this.http.post<ApiResponse<LocationModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<LocationModel>): Observable<ApiResponse<LocationModel>> {
    return this.http.put<ApiResponse<LocationModel>>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
