export interface Account {
  id: string;
  name: string;
  type: '' |'Checking' | 'Savings' | 'Cash' | 'CreditCard' | 'Loan' | 'Mortgage' | 'Investment';
  currency: string;
  isDeleted?: boolean;
}