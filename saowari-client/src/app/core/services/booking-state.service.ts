import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TripSearchResult } from '../models/business.model';

export interface SelectedTripState {
  schedule: TripSearchResult | null;
  seatIds: number[];
  seatNumbers: string[];
  selectedBoardingPoint?: string;
}

export interface BookingState {
  tripType: 'one-way' | 'round-way';
  outbound: SelectedTripState;
  return: SelectedTripState;
}

@Injectable({
  providedIn: 'root'
})
export class BookingStateService {
  private initialState: BookingState = {
    tripType: 'one-way',
    outbound: { schedule: null, seatIds: [], seatNumbers: [], selectedBoardingPoint: '' },
    return: { schedule: null, seatIds: [], seatNumbers: [], selectedBoardingPoint: '' }
  };

  private stateSubject = new BehaviorSubject<BookingState>(this.initialState);
  state$ = this.stateSubject.asObservable();

  constructor() {}

  get currentState(): BookingState {
    return this.stateSubject.value;
  }

  setTripType(type: 'one-way' | 'round-way') {
    this.stateSubject.next({ ...this.currentState, tripType: type });
  }

  setOutboundTrip(schedule: TripSearchResult) {
    this.stateSubject.next({
      ...this.currentState,
      outbound: { ...this.currentState.outbound, schedule }
    });
  }

  setReturnTrip(schedule: TripSearchResult) {
    this.stateSubject.next({
      ...this.currentState,
      return: { ...this.currentState.return, schedule }
    });
  }

  setOutboundSeats(seatIds: number[], seatNumbers: string[]) {
    this.stateSubject.next({
      ...this.currentState,
      outbound: { ...this.currentState.outbound, seatIds, seatNumbers }
    });
  }

  setReturnSeats(seatIds: number[], seatNumbers: string[]) {
    this.stateSubject.next({
      ...this.currentState,
      return: { ...this.currentState.return, seatIds, seatNumbers }
    });
  }

  setOutboundBoardingPoint(point: string) {
    this.stateSubject.next({
      ...this.currentState,
      outbound: { ...this.currentState.outbound, selectedBoardingPoint: point }
    });
  }

  setReturnBoardingPoint(point: string) {
    this.stateSubject.next({
      ...this.currentState,
      return: { ...this.currentState.return, selectedBoardingPoint: point }
    });
  }

  clearState() {
    this.stateSubject.next(this.initialState);
  }

  isRoundTripReadyForCheckout(): boolean {
    const s = this.currentState;
    if (s.tripType === 'one-way') {
      return !!s.outbound.schedule && s.outbound.seatIds.length > 0;
    } else {
      return !!s.outbound.schedule && s.outbound.seatIds.length > 0 &&
             !!s.return.schedule && s.return.seatIds.length > 0;
    }
  }
}
