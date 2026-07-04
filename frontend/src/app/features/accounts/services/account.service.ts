import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Account } from '../models/account.model';
import { Observable } from 'rxjs';

@Service()
export class AccountService {
    private http = inject(HttpClient);

    getAccounts() : Observable<Account[]> {
        return this.http.get<Account[]>('/api/accounts');
    }

    getAccountById(accountId: string) : Observable<Account> {
        return this.http.get<Account>(`/api/accounts/${accountId}`);
    }

    getCurrencies() : Observable<string[]> {
        return this.http.get<string[]>('/api/accounts/currencies');
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
}
