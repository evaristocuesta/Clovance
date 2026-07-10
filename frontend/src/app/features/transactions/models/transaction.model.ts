export interface Transaction {
  id: string;
  accountId: string;
  amount: number;
  date: Date;
  description?: string;
  relatedTransactionId?: string;
}