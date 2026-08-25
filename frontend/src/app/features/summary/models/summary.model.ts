export interface AccountBalancePoint {
  accountId: string;
  balance: number;
}

export interface AccountCashflowBreakdown {
  accountId: string;
  income: number;
  expenses: number;
}

export interface DailyBalancePoint {
  date: string;
  balance: number;
  byAccount: AccountBalancePoint[] | null;
}

export interface GetDailyBalanceResult {
  dailyBalance: DailyBalancePoint[];
}

export interface DailyCashflowPoint {
  date: string;
  income: number;
  expenses: number;
  byAccount: AccountCashflowBreakdown[] | null;
}

export interface GetDailyCashflowResult {
  dailyClashFlow: DailyCashflowPoint[];
}

export interface MonthlyBalancePoint {
  year: number;
  month: number;
  balance: number;
  byAccount: AccountBalancePoint[] | null;
}

export interface GetMonthlyBalanceResult {
  points: MonthlyBalancePoint[];
}

export interface MonthlyCashflowPoint {
  year: number;
  month: number;
  income: number;
  expenses: number;
  byAccount: AccountCashflowBreakdown[] | null;
}

export interface GetMonthlyCashflowResult {
  points: MonthlyCashflowPoint[];
}

export interface SummaryQueryParams {
  accountId?: string;
  month?: number;
  year?: number;
}
