import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { UserModel } from '../../models/auth.model';
import { BookingModel, InvoiceModel } from '../../models/transaction.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<UserModel[]>> {
    return this.http.get<ApiResponse<UserModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<UserModel>> {
    return this.http.get<ApiResponse<UserModel>>(`${this.apiUrl}/${id}`);
  }

  getByEmail(email: string): Observable<ApiResponse<UserModel>> {
    return this.http.get<ApiResponse<UserModel>>(`${this.apiUrl}/by-email/${email}`);
  }

  getMe(): Observable<ApiResponse<UserModel>> {
    return this.http.get<ApiResponse<UserModel>>(`${this.apiUrl}/me`);
  }

  create(data: any): Observable<ApiResponse<UserModel>> {
    return this.http.post<ApiResponse<UserModel>>(this.apiUrl, data);
  }

  update(id: number, data: any): Observable<ApiResponse<UserModel>> {
    return this.http.put<ApiResponse<UserModel>>(`${this.apiUrl}/${id}`, data);
  }

  updateProfile(data: any): Observable<ApiResponse<UserModel>> {
    return this.http.put<ApiResponse<UserModel>>(`${this.apiUrl}/me`, data);
  }

  changePassword(userId: number, currentPassword: string, newPassword: string): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/me/change-password`, { currentPassword, newPassword });
  }

  patchActive(id: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/active`, isActive);
  }

  getMyBookings(): Observable<ApiResponse<BookingModel[]>> {
    return this.http.get<ApiResponse<BookingModel[]>>(`${this.apiUrl}/me/bookings`);
  }

  getMyInvoice(bookingId: number): Observable<ApiResponse<InvoiceModel>> {
    return this.http.get<ApiResponse<InvoiceModel>>(`${this.apiUrl}/me/bookings/${bookingId}/invoice`);
  }
}
