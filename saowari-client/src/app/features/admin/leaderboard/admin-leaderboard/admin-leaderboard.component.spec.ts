import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminLeaderboardComponent } from './admin-leaderboard.component';

describe('AdminLeaderboardComponent', () => {
  let component: AdminLeaderboardComponent;
  let fixture: ComponentFixture<AdminLeaderboardComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdminLeaderboardComponent]
    });
    fixture = TestBed.createComponent(AdminLeaderboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
