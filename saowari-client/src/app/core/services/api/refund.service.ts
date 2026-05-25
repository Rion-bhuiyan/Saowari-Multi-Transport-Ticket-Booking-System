import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { RefundModel, RefundPreview } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class RefundService {
  private apiUrl = `${environment.apiUrl}/refunds`;

  constructor(private http: HttpClient) {}

  getByUser(userId: number): Observable<ApiResponse<RefundModel[]>> {
    return this.http.get<ApiResponse<RefundModel[]>>(`${this.apiUrl}/my`);
  }

  getAll(): Observable<ApiResponse<RefundModel[]>> {
    return this.http.get<ApiResponse<RefundModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<RefundModel>> {
    return this.http.get<ApiResponse<RefundModel>>(`${this.apiUrl}/${id}`);
  }

  getByBooking(bookingId: number): Observable<ApiResponse<RefundModel[]>> {
    return this.http.get<ApiResponse<RefundModel[]>>(`${this.apiUrl}/by-booking/${bookingId}`);
  }

  request(data: Partial<RefundModel>): Observable<ApiResponse<RefundModel>> {
    return this.http.post<ApiResponse<RefundModel>>(this.apiUrl, data);
  }

  requestCustomerRefund(bookingId: number, remarks: string): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/request`, { bookingId, remarks });
  }

  patchStatus(id: number, statusId: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, { statusId });
  }

  calculate(bookingId: number): Observable<ApiResponse<RefundPreview>> {
    return this.http.get<ApiResponse<RefundPreview>>(`${this.apiUrl}/calculate?bookingId=${bookingId}`);
  }
}
