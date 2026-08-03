export interface Transfer {
  date: Date;
  description: string;
  amount: number;
  fromAccountId: string;
  toAccountId: string;
}