import { Component, OnInit, signal } from '@angular/core';
import { Account } from '@features/accounts/models/account.model';
import { Transaction } from '../models/transaction.model';

export interface LoanPaymentFormData {
  toTransaction?: Transaction;
  fromTransaction?: Transaction;
  accounts: Account[];
}

@Component({
  selector: 'app-loan-payment-form',
  imports: [],
  templateUrl: './loan-payment-form.html',
  styleUrl: './loan-payment-form.css',
})
export class LoanPaymentForm implements OnInit {
  errorMessage = signal('');

  

  ngOnInit(): void {
  }
  
}
