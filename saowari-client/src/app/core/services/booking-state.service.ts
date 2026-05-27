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
  private readonly STORAGE_KEY = 'saowari_booking_state';

  private initialState: BookingState = {
    tripType: 'one-way',
    outbound: { schedule: null, seatIds: [], seatNumbers: [], selectedBoardingPoint: '' },
    return: { schedule: null, seatIds: [], seatNumbers: [], selectedBoardingPoint: '' }
  };

  private stateSubject = new BehaviorSubject<BookingState>(this.loadFromStorage());
  state$ = this.stateSubject.asObservable();

  constructor() {
    this.state$.subscribe(state => {
      this.saveToStorage(state);
    });
  }

  private loadFromStorage(): BookingState {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        return JSON.parse(stored) as BookingState;
      }
    } catch (e) {
      console.error('Failed to load booking state from local storage', e);
    }
    return this.initialState;
  }

  private saveToStorage(state: BookingState) {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
    } catch (e) {
      console.error('Failed to save booking state to local storage', e);
    }
  }

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
    try {
      localStorage.removeItem(this.STORAGE_KEY);
    } catch (e) {}
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
