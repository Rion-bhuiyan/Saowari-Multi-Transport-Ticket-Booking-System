import { TestBed } from '@angular/core/testing';

import { TrafficAnalyticsService } from './traffic-analytics.service';

describe('TrafficAnalyticsService', () => {
  let service: TrafficAnalyticsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TrafficAnalyticsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
