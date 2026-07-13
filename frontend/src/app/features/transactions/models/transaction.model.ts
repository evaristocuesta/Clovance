export interface Transaction {
  id: string;
  accountId: string;
  accountName: string;
  currency: string;
  amount: number;
  date: Date;
  description: string;
  type: 'income' | 'expense' | 'transfer';
  relatedTransactionId?: string;
}