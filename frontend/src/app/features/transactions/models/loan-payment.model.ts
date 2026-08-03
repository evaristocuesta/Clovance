import { Transfer } from './transfer.model';

export interface LoanPayment extends Transfer {
    principalAmount: number;
}