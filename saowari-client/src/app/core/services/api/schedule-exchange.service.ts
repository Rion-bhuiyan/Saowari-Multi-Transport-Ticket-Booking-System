import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ScheduleExchangeRequest {
  id: number;
  requesterId: number;
  requesterName: string;
  targetUserId: number;
  targetUserName: string;
  requesterScheduleId: number;
  requesterScheduleRoute: string;
  requesterScheduleDeparture: string;
  targetScheduleId: number;
  targetScheduleRoute: string;
  targetScheduleDeparture: string;
  status: string;
  remarks?: string;
  managerRemarks?: string;
  createdAt: string;
  peerRespondedAt?: string;
  managerRespondedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ScheduleExchangeService {
  private baseUrl = 'http://localhost:5293/api/schedule-exchanges';

  constructor(private http: HttpClient) {}

  public getAll(): Observable<any> {
    return this.http.get<any>(this.baseUrl);
  }

  public create(payload: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, payload);
  }

  public peerRespond(id: number, accept: boolean): Observable<any> {
    return this.http.patch<any>(`${this.baseUrl}/${id}/peer-respond`, { accept });
  }

  public managerRespond(id: number, status: string, managerRemarks: string = ''): Observable<any> {
    return this.http.patch<any>(`${this.baseUrl}/${id}/manager-respond`, { status, managerRemarks });
  }
}
