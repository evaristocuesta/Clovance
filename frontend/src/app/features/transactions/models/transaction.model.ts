export interface Transaction {
  id: string;
  accountId: string;
  accountName: string;
  currency: string;
  amount: number;
  principalAmount?: number;
  date: Date;
  description: string;
  type: 'Income' | 'Expense' | 'Transfer' | 'OpeningBalance' | 'LoanPayment';
  relatedTransactionId?: string;
}