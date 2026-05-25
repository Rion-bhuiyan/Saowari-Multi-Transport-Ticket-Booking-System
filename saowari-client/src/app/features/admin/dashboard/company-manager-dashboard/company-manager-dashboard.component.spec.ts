import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyManagerDashboardComponent } from './company-manager-dashboard.component';

describe('CompanyManagerDashboardComponent', () => {
  let component: CompanyManagerDashboardComponent;
  let fixture: ComponentFixture<CompanyManagerDashboardComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CompanyManagerDashboardComponent]
    });
    fixture = TestBed.createComponent(CompanyManagerDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
