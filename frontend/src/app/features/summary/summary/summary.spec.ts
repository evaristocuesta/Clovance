import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Summary } from './summary';
import { AccountService } from '@features/accounts/services/account.service';
import { SummaryService } from '../services/summary.service';

describe('Summary', () => {
  let component: Summary;
  let fixture: ComponentFixture<Summary>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Summary],
      providers: [
        {
          provide: AccountService,
          useValue: {
            getAccounts: () => of([]),
            getCurrencies: () => of([]),
          },
        },
        {
          provide: SummaryService,
          useValue: {
            getMonthlyBalance: () => of({ points: [] }),
            getMonthlyCashflow: () => of({ points: [] }),
            getDailyBalance: () => of({ dailyBalance: [] }),
            getDailyCashflow: () => of({ dailyClashFlow: [] }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Summary);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
