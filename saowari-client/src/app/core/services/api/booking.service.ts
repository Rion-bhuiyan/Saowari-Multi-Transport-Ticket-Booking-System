import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { BookingCreateDto, BookingModel, FareSummary } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private apiUrl = `${environment.apiUrl}/bookings`;
  private flowUrl = `${environment.apiUrl}/bookings/flow`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<BookingModel[]>> {
    return this.http.get<ApiResponse<BookingModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<BookingModel>> {
    return this.http.get<ApiResponse<BookingModel>>(`${this.apiUrl}/${id}`);
  }

  getByCode(code: string): Observable<ApiResponse<BookingModel>> {
    return this.http.get<ApiResponse<BookingModel>>(`${this.apiUrl}/by-code/${code}`);
  }

  getMy(): Observable<ApiResponse<BookingModel[]>> {
    return this.http.get<ApiResponse<BookingModel[]>>(`${this.apiUrl}/my`);
  }

  getByUser(userId: number): Observable<ApiResponse<BookingModel[]>> {
    return this.getMy();
  }

  getBySchedule(scheduleId: number): Observable<ApiResponse<BookingModel[]>> {
    return this.http.get<ApiResponse<BookingModel[]>>(`${this.apiUrl}/by-schedule/${scheduleId}`);
  }

  create(data: BookingCreateDto): Observable<ApiResponse<BookingModel>> {
    return this.http.post<ApiResponse<BookingModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<BookingModel>): Observable<ApiResponse<BookingModel>> {
    return this.http.put<ApiResponse<BookingModel>>(`${this.apiUrl}/${id}`, data);
  }

  patchStatus(id: number, statusId: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, statusId);
  }

  cancel(id: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/cancel`, {});
  }

  requestCancel(id: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/${id}/request-cancel`, {});
  }

  verifyCancel(id: number, otp: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/${id}/verify-cancel`, { otp });
  }

  validateSeats(scheduleId: number, seatIds: number[]): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.flowUrl}/validate-seats`, { scheduleId, seatIds });
  }

  validateCoupon(data: { scheduleId: number, couponCode: string, totalTicketAmount: number }): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${environment.apiUrl}/discounts/validate`, data);
  }

  getFareSummary(scheduleId: number, seatIds: string, discountId?: number): Observable<ApiResponse<FareSummary>> {
    let params = new HttpParams()
      .set('scheduleId', scheduleId.toString())
      .set('seatIds', seatIds);
    if (discountId) params = params.set('discountId', discountId.toString());
    
    return this.http.get<ApiResponse<FareSummary>>(`${this.flowUrl}/fare-summary`, { params });
  }

  reschedule(id: number, newScheduleId: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.flowUrl}/reschedule/${id}`, { newScheduleId });
  }
}
