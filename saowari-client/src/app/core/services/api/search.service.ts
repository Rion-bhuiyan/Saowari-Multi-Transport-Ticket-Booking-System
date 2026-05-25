import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';
import { TripSearchResult } from '../../models/business.model';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private apiUrl = `${environment.apiUrl}/search`;

  constructor(private http: HttpClient) {}

  searchTrips(params: any): Observable<ApiResponse<TripSearchResult[]>> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== null && params[key] !== undefined) {
        httpParams = httpParams.append(key, params[key]);
      }
    });
    return this.http.get<ApiResponse<TripSearchResult[]>>(`${this.apiUrl}/trips`, { params: httpParams });
  }

  getSeatMap(scheduleId: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/seat-map/${scheduleId}`);
  }
}
