import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { SliderImageModel } from '../../models/master.model';

@Injectable({ providedIn: 'root' })
export class SliderImageService {
  private apiUrl = `${environment.apiUrl}/slider-images`;

  constructor(private http: HttpClient) {}

  // Get active slides for public home page
  getActive(): Observable<ApiResponse<SliderImageModel[]>> {
    return this.http.get<ApiResponse<SliderImageModel[]>>(this.apiUrl);
  }

  // Get all slides for admin view (active & inactive)
  getAll(): Observable<ApiResponse<SliderImageModel[]>> {
    return this.http.get<ApiResponse<SliderImageModel[]>>(`${this.apiUrl}/all`);
  }

  getById(id: number): Observable<ApiResponse<SliderImageModel>> {
    return this.http.get<ApiResponse<SliderImageModel>>(`${this.apiUrl}/${id}`);
  }

  create(data: FormData): Observable<ApiResponse<SliderImageModel>> {
    return this.http.post<ApiResponse<SliderImageModel>>(this.apiUrl, data);
  }

  update(id: number, data: FormData): Observable<ApiResponse<SliderImageModel>> {
    return this.http.put<ApiResponse<SliderImageModel>>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
