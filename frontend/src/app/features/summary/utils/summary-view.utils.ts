import { Account } from '@features/accounts/models/account.model';
import { BalanceChartPoint, CashflowChartPoint } from '../models/summary-chart.model';

export type SummaryTab = 'assets' | 'liabilities';

export function getSummaryAccounts(accounts: Account[], tab: SummaryTab): Account[] {
  const isMatchingType = tab === 'assets' ? Account.isAsset : Account.isLiability;
  return accounts.filter((account) => isMatchingType(account));
}

export function toBalanceViewPoints(points: BalanceChartPoint[], tab: SummaryTab): BalanceChartPoint[] {
  if (tab === 'assets') return points;

  return points.map((point) => ({
    ...point,
    total: Math.abs(point.total),
    byAccount: point.byAccount?.map((account) => ({
      ...account,
      balance: Math.abs(account.balance),
    })) ?? null,
  }));
}

export function toCashflowViewPoints(points: CashflowChartPoint[], tab: SummaryTab): CashflowChartPoint[] {
  if (tab === 'assets') return points;

  return points.map((point) => ({
    ...point,
    income: Math.abs(point.income),
    expenses: -Math.abs(point.expenses),
    byAccount: point.byAccount?.map((account) => ({
      ...account,
      income: Math.abs(account.income),
      expenses: -Math.abs(account.expenses),
    })) ?? null,
  }));
}
