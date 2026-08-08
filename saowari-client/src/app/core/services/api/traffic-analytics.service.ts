import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TrafficAnalyticsService {
  constructor(private http: HttpClient) { }

  getAnalytics(): Observable<any> {
    return this.http.get(`${environment.apiUrl}/TrafficAnalytics`);
  }
}
