import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../models/common.model';

export interface SeatPricingModel {
  pricingID?: number;
  vehicleId: number;
  seatClassId: number;
  seatClassName?: string;
  price: number;
  lastUpdate?: string;
}

@Injectable({ providedIn: 'root' })
export class SeatPricingService {
  private apiUrl = `${environment.apiUrl}/seatpricings`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<SeatPricingModel[]>> {
    return this.http.get<ApiResponse<SeatPricingModel[]>>(this.apiUrl);
  }

  getById(id: number): Observable<ApiResponse<SeatPricingModel>> {
    return this.http.get<ApiResponse<SeatPricingModel>>(`${this.apiUrl}/${id}`);
  }

  getByVehicle(vehicleId: number): Observable<ApiResponse<SeatPricingModel[]>> {
    return this.http.get<ApiResponse<SeatPricingModel[]>>(`${this.apiUrl}/vehicle/${vehicleId}`);
  }

  create(data: Partial<SeatPricingModel>): Observable<ApiResponse<SeatPricingModel>> {
    return this.http.post<ApiResponse<SeatPricingModel>>(this.apiUrl, data);
  }

  update(id: number, data: Partial<SeatPricingModel>): Observable<ApiResponse<SeatPricingModel>> {
    return this.http.put<ApiResponse<SeatPricingModel>>(`${this.apiUrl}/${id}`, data);
  }

  bulkUpsert(vehicleId: number, pricings: { seatClassId: number; price: number }[]): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/vehicle/${vehicleId}`, pricings);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
