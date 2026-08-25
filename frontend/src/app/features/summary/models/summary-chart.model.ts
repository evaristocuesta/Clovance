import { AccountBalancePoint, AccountCashflowBreakdown } from './summary.model';

export interface BalanceChartPoint {
  label: string;
  total: number;
  byAccount: AccountBalancePoint[] | null;
}

export interface CashflowChartPoint {
  label: string;
  income: number;
  expenses: number;
  byAccount: AccountCashflowBreakdown[] | null;
}
