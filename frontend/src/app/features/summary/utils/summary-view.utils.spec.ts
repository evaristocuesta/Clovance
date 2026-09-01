import { Account } from '@features/accounts/models/account.model';
import { getSummaryAccounts, toBalanceViewPoints, toCashflowViewPoints } from './summary-view.utils';

const accounts: Account[] = [
  { id: 'checking', name: 'Checking', type: 'Checking', currency: 'EUR' },
  { id: 'loan', name: 'Loan', type: 'Loan', currency: 'EUR' },
  { id: 'deleted-card', name: 'Deleted card', type: 'CreditCard', currency: 'EUR', isDeleted: true },
];

describe('summary view utilities', () => {
  it('filters accounts by summary category', () => {
    expect(getSummaryAccounts(accounts, 'assets').map((account) => account.id)).toEqual(['checking']);
    expect(getSummaryAccounts(accounts, 'liabilities').map((account) => account.id)).toEqual(['loan', 'deleted-card']);
  });

  it('keeps asset points unchanged', () => {
    const points = [{ label: '2026-01', total: 100, byAccount: [{ accountId: 'checking', balance: 100 }] }];

    expect(toBalanceViewPoints(points, 'assets')).toBe(points);
  });

  it('converts liability balances to positive presentation values without mutating API data', () => {
    const points = [{ label: '2026-01', total: -1200, byAccount: [{ accountId: 'loan', balance: -1200 }] }];

    const result = toBalanceViewPoints(points, 'liabilities');

    expect(result).toEqual([{ label: '2026-01', total: 1200, byAccount: [{ accountId: 'loan', balance: 1200 }] }]);
    expect(points[0].total).toBe(-1200);
    expect(points[0].byAccount?.[0].balance).toBe(-1200);
  });

  it('converts liability cashflow values to positive presentation values', () => {
    const points = [{
      label: '2026-01',
      income: -50,
      expenses: -200,
      byAccount: [{ accountId: 'loan', income: -50, expenses: -200 }],
    }];

    expect(toCashflowViewPoints(points, 'liabilities')).toEqual([{
      label: '2026-01',
      income: 50,
      expenses: -200,
      byAccount: [{ accountId: 'loan', income: 50, expenses: -200 }],
    }]);
  });
});
