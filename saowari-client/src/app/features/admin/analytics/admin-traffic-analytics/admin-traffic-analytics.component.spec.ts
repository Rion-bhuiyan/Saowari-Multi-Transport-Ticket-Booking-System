import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminTrafficAnalyticsComponent } from './admin-traffic-analytics.component';

describe('AdminTrafficAnalyticsComponent', () => {
  let component: AdminTrafficAnalyticsComponent;
  let fixture: ComponentFixture<AdminTrafficAnalyticsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdminTrafficAnalyticsComponent]
    });
    fixture = TestBed.createComponent(AdminTrafficAnalyticsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
