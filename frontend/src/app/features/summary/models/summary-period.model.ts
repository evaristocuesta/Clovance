export interface SummaryPeriod {
  year: number;
  month: number;
  viewMode: 'monthly' | 'daily';
  accountId: string | null;
  currency: string;
}
