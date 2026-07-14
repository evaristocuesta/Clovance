import { Transaction } from "./transaction.model";

export interface Transfer {
  date: Date;
  description: string;
  amount: number;
  fromAccountId: string;
  toAccountId: string;
}