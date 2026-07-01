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

    createAccount(account: Account) : Observable<Account> {
        return this.http.post<Account>('/api/accounts', account);
    }

    updateAccount(accountId: string, account: Account) : Observable<Account> {
        return this.http.put<Account>(`/api/accounts/${accountId}`, account);
    }

    deleteAccount(accountId: string) : Observable<void> {
        return this.http.delete<void>(`/api/accounts/${accountId}`);
    }
}
