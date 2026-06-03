import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private apiUrl = `${environment.apiUrl}/settings`;

  constructor(private http: HttpClient) {}

  getLogo(): Observable<ApiResponse<string>> {
    return this.http.get<ApiResponse<string>>(`${this.apiUrl}/logo`);
  }

  uploadLogo(formData: FormData): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/logo`, formData);
  }

  getTicketBackground(): Observable<ApiResponse<string>> {
    return this.http.get<ApiResponse<string>>(`${this.apiUrl}/ticket-background`);
  }

  uploadTicketBackground(formData: FormData): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/ticket-background`, formData);
  }

  deleteTicketBackground(): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/ticket-background`);
  }

  // --- Admin System Settings ---
  private adminApiUrl = `${environment.apiUrl}/adminsettings`;

  getSystemSettings(): Observable<ApiResponse<Record<string, string>>> {
    return this.http.get<ApiResponse<Record<string, string>>>(this.adminApiUrl);
  }

  getPublicSystemSettings(): Observable<ApiResponse<Record<string, string>>> {
    return this.http.get<ApiResponse<Record<string, string>>>(`${this.apiUrl}/system`);
  }

  updateSystemSettings(settings: Record<string, string>): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(this.adminApiUrl, settings);
  }
}
