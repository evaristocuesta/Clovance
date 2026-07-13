export type TransactionType = 'income' | 'expense' | 'transfer';
export type TransactionTypeCode = '0' | '1' | '2';

export const TRANSACTION_TYPE_MAP: Record<string, TransactionType> = {
  '0': 'income',
  '1': 'expense',
  '2': 'transfer',
  income: 'income',
  expense: 'expense',
  transfer: 'transfer',
};

export const TRANSACTION_TYPE_CODE_MAP: Record<TransactionType, TransactionTypeCode> = {
  income: '0',
  expense: '1',
  transfer: '2',
};

export function toTransactionTypeCode(type: unknown): TransactionTypeCode | null {
  const normalizedType = TRANSACTION_TYPE_MAP[String(type).trim().toLowerCase()];

  if (!normalizedType) {
    return null;
  }

  return TRANSACTION_TYPE_CODE_MAP[normalizedType];
}

export function toTransactionType(type: unknown): TransactionType | null {
  const normalizedType = TRANSACTION_TYPE_MAP[String(type).trim().toLowerCase()];

  if (!normalizedType) {
    return null;
  }

  return normalizedType;
}
