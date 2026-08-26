import { Currency } from '@features/accounts/models/currency.model';

type CurrencyOptions = Currency[] | Record<string, string>;

export function getCurrencySymbol(currency: string, options: CurrencyOptions): string {
  const code = currency.toUpperCase();

  if (Array.isArray(options)) {
    return options.find((option) => option.code.toUpperCase() === code)?.symbol ?? code;
  }

  return options[code] ?? code;
}

export function formatCurrency(
  value: number,
  currency: string,
  options: CurrencyOptions,
  language = 'en',
  includeSign = false,
): string {
  const symbol = getCurrencySymbol(currency, options);
  const locale = language.toLowerCase().startsWith('es') ? 'es-ES' : 'en-US';
  const parts = new Intl.NumberFormat(locale, {
    style: 'currency',
    currency: currency.toUpperCase(),
    currencyDisplay: 'symbol',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).formatToParts(Math.abs(value));
  const amount = parts.map((part) => part.type === 'currency' ? symbol : part.value).join('');
  const sign = includeSign && value > 0 ? '+' : value < 0 ? '-' : '';

  return `${sign}${amount}`;
}
