import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShowUserInvitation } from './show-user-invitation';

describe('ShowUserInvitation', () => {
  let component: ShowUserInvitation;
  let fixture: ComponentFixture<ShowUserInvitation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShowUserInvitation],
    }).compileComponents();

    fixture = TestBed.createComponent(ShowUserInvitation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
