import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminVehicleTypesComponent } from './admin-vehicle-types.component';

describe('AdminVehicleTypesComponent', () => {
  let component: AdminVehicleTypesComponent;
  let fixture: ComponentFixture<AdminVehicleTypesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AdminVehicleTypesComponent]
    });
    fixture = TestBed.createComponent(AdminVehicleTypesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
