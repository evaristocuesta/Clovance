export interface Account {
  id: string;
  name: string;
  type: '' |'Checking' | 'Savings' | 'Cash' | 'CreditCard' | 'Loan' | 'Mortgage' | 'Investment';
  currency: string;
  isDeleted?: boolean;
}

export namespace Account {
  const ACCOUNT_CATEGORY: Record<Exclude<Account['type'], ''>, 'asset' | 'liability'> = {
    Checking: 'asset',
    Savings: 'asset',
    Cash: 'asset',
    Investment: 'asset',
    CreditCard: 'liability',
    Loan: 'liability',
    Mortgage: 'liability',
  };

  export function isLiability(account: Account): boolean {
    return account.type !== '' && ACCOUNT_CATEGORY[account.type] === 'liability';
  }

  export function isAsset(account: Account): boolean {
    return account.type !== '' && ACCOUNT_CATEGORY[account.type] === 'asset';
  }

  export function canBeUsedForIncomeOrExpense(account: Account): boolean {
    return account.type !== '' && (isAsset(account) || account.type === 'CreditCard');
  }

  export function canBeUsedForTransfer(account: Account): boolean {
    return account.type !== '' && isAsset(account);
  }

  export function canBeUsedForFromLoanPayment(account: Account): boolean {
    return account.type !== '' && (isAsset(account) || account.type === 'CreditCard');
  }

  export function canBeUsedForToLoanPayment(account: Account): boolean {
    return account.type !== '' && isLiability(account);
  }
}