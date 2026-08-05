import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DIALOG_DATA } from '@angular/cdk/dialog';

import { LoanPaymentForm } from './loan-payment-form';

describe('LoanPaymentForm', () => {
  let component: LoanPaymentForm;
  let fixture: ComponentFixture<LoanPaymentForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanPaymentForm],
      providers: [{ provide: DIALOG_DATA, useValue: { accounts: [] } }],
    }).compileComponents();

    fixture = TestBed.createComponent(LoanPaymentForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
