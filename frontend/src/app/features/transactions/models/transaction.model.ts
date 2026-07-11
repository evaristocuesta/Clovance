export interface Transaction {
  id: string;
  accountId: string;
  accountName: string;
  amount: number;
  date: Date;
  description: string;
  type: string;
  relatedTransactionId?: string;
}