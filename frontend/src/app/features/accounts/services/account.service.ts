import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Account } from '../models/account.model';
import { Observable, of, tap } from 'rxjs';
import { Currency } from '../models/currency.model';

@Service()
export class AccountService {
    private http = inject(HttpClient);
    private currenciesCache: Currency[] | null = null;

    getAccounts() : Observable<Account[]> {
        return this.http.get<Account[]>('/api/accounts');
    }

    getAccountById(accountId: string) : Observable<Account> {
        return this.http.get<Account>(`/api/accounts/${accountId}`);
    }

    getCurrencies() : Observable<Currency[]> {
        if (this.currenciesCache) {
            return of(this.currenciesCache);
        }
        
        return this.http.get<Currency[]>('/api/accounts/currencies').pipe(
            tap((currencies) => this.currenciesCache = currencies)
        );
    }

    createAccount(account: Account) : Observable<Account> {
        return this.http.post<Account>('/api/accounts', account);
    }

    updateAccount(account: Account) : Observable<Account> {
        return this.http.put<Account>(`/api/accounts/${account.id}`, account);
    }

    deleteAccount(accountId: string) : Observable<void> {
        return this.http.delete<void>(`/api/accounts/${accountId}`);
    }

    restoreAccount(accountId: string) : Observable<void> {
        return this.http.put<void>(`/api/accounts/${accountId}/restore`, {});
    }
}
