import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ScheduleApplication {
  id: number;
  requesterId: number;
  requesterName: string;
  companyId: number;
  companyName: string;
  routeId: number;
  routeName: string;
  vehicleId: number;
  vehicleName: string;
  vehicleNumber: string;
  departureDateTime: string;
  arrivalDateTime: string;
  status: string;
  remarks?: string;
  managerRemarks?: string;
  createdAt: string;
  respondedAt?: string;
  createdScheduleId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ScheduleApplicationService {
  private baseUrl = 'http://localhost:5293/api/schedule-applications';

  constructor(private http: HttpClient) {}

  public getAll(): Observable<any> {
    return this.http.get<any>(this.baseUrl);
  }

  public create(payload: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, payload);
  }

  public respond(id: number, status: string, managerRemarks: string = ''): Observable<any> {
    return this.http.patch<any>(`${this.baseUrl}/${id}/respond`, { status, managerRemarks });
  }
}
