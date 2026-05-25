import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminScheduleSeatMapComponent } from './admin-schedule-seat-map.component';

describe('AdminScheduleSeatMapComponent', () => {
  let component: AdminScheduleSeatMapComponent;
  let fixture: ComponentFixture<AdminScheduleSeatMapComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AdminScheduleSeatMapComponent]
    });
    fixture = TestBed.createComponent(AdminScheduleSeatMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
