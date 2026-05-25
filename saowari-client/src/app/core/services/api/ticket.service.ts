import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { TicketModel, TicketVerification } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private apiUrl = `${environment.apiUrl}/tickets`;

  constructor(private http: HttpClient) {}

  getByUser(userId: number): Observable<ApiResponse<TicketModel[]>> {
    return this.http.get<ApiResponse<TicketModel[]>>(`${this.apiUrl}/my`);
  }

  getByBooking(bookingId: number): Observable<ApiResponse<TicketModel[]>> {
    return this.http.get<ApiResponse<TicketModel[]>>(`${this.apiUrl}/by-booking/${bookingId}`);
  }

  getByCode(code: string): Observable<ApiResponse<TicketModel>> {
    return this.http.get<ApiResponse<TicketModel>>(`${this.apiUrl}/by-code/${code}`);
  }

  verify(code: string): Observable<ApiResponse<TicketVerification>> {
    return this.http.get<ApiResponse<TicketVerification>>(`${this.apiUrl}/business/verify/${code}`);
  }

  issueForBooking(bookingId: number): Observable<ApiResponse<TicketModel[]>> {
    return this.http.post<ApiResponse<TicketModel[]>>(`${this.apiUrl}/business/issue-for-booking/${bookingId}`, {});
  }

  markUsed(id: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/used`, {});
  }

  scan(code: string): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/business/scan/${code}`, {});
  }
}
