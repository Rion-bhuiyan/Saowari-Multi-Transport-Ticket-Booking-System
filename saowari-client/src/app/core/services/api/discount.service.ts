import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { DiscountModel } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class DiscountService {
  private apiUrl = `${environment.apiUrl}/discounts`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<DiscountModel[]>> {
    return this.http.get<ApiResponse<DiscountModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<DiscountModel>> {
    return this.http.get<ApiResponse<DiscountModel>>(`${this.apiUrl}/${id}`);
  }

  getActive(): Observable<ApiResponse<DiscountModel[]>> {
    return this.http.get<ApiResponse<DiscountModel[]>>(`${this.apiUrl}/active`);
  }

  getByCompany(companyId: number): Observable<ApiResponse<DiscountModel[]>> {
    return this.http.get<ApiResponse<DiscountModel[]>>(`${this.apiUrl}/by-company/${companyId}`);
  }

  validate(code: string, scheduleId: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/validate`, { code, scheduleId });
  }

  create(data: Partial<DiscountModel>): Observable<ApiResponse<DiscountModel>> {
    return this.http.post<ApiResponse<DiscountModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<DiscountModel>): Observable<ApiResponse<DiscountModel>> {
    return this.http.put<ApiResponse<DiscountModel>>(`${this.apiUrl}/${id}`, data);
  }

  patchActive(id: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, isActive);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
