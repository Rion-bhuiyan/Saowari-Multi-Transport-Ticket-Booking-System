import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketViewComponent } from './ticket-view.component';

describe('TicketViewComponent', () => {
  let component: TicketViewComponent;
  let fixture: ComponentFixture<TicketViewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TicketViewComponent]
    });
    fixture = TestBed.createComponent(TicketViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
