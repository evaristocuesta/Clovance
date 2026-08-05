import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DIALOG_DATA } from '@angular/cdk/dialog';

import { TransferForm } from './transfer-form';

describe('TransferForm', () => {
  let component: TransferForm;
  let fixture: ComponentFixture<TransferForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransferForm],
      providers: [{ provide: DIALOG_DATA, useValue: { accounts: [] } }],
    }).compileComponents();

    fixture = TestBed.createComponent(TransferForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
