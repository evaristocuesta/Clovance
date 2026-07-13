import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TransactionsFilter } from './transactions-filter';

describe('TransactionsFilter', () => {
  let component: TransactionsFilter;
  let fixture: ComponentFixture<TransactionsFilter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransactionsFilter],
    }).compileComponents();

    fixture = TestBed.createComponent(TransactionsFilter);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
