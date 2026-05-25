import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';

export interface PaymentMethodModel {
  paymentMethodId: number;
  paymentMethodName: string;
  processingFeePercent: number;
  vatPercent: number;
  logoUrl?: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class PaymentMethodService {
  private apiUrl = `${environment.apiUrl}/paymentmethods`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<PaymentMethodModel[]>> {
    return this.http.get<ApiResponse<PaymentMethodModel[]>>(this.apiUrl);
  }

  getActive(): Observable<ApiResponse<PaymentMethodModel[]>> {
    return this.http.get<ApiResponse<PaymentMethodModel[]>>(`${this.apiUrl}/active`);
  }

  getById(id: number): Observable<ApiResponse<PaymentMethodModel>> {
    return this.http.get<ApiResponse<PaymentMethodModel>>(`${this.apiUrl}/${id}`);
  }

  create(formData: FormData): Observable<ApiResponse<PaymentMethodModel>> {
    return this.http.post<ApiResponse<PaymentMethodModel>>(this.apiUrl, formData);
  }

  update(id: number, formData: FormData): Observable<ApiResponse<PaymentMethodModel>> {
    return this.http.put<ApiResponse<PaymentMethodModel>>(`${this.apiUrl}/${id}`, formData);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
