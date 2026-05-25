import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { PaymentModel } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<PaymentModel[]>> {
    return this.http.get<ApiResponse<PaymentModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<PaymentModel>> {
    return this.http.get<ApiResponse<PaymentModel>>(`${this.apiUrl}/${id}`);
  }

  getByBooking(bookingId: number): Observable<ApiResponse<PaymentModel[]>> {
    return this.http.get<ApiResponse<PaymentModel[]>>(`${this.apiUrl}/by-booking/${bookingId}`);
  }

  create(data: Partial<PaymentModel>): Observable<ApiResponse<PaymentModel>> {
    return this.http.post<ApiResponse<PaymentModel>>(this.apiUrl, data);
  }

  patchStatus(id: number, statusId: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, statusId);
  }
}
