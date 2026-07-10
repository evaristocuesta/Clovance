import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Transaction } from '../models/transaction.model';
import { map, Observable } from 'rxjs';

export interface TransactionFilters {
    year?: number | null;
    month?: number | null;
    accountId?: string | null;
    description?: string | null;
    cursorDate?: string | null;
    cursorId?: string | null;
    pageSize?: number | null;
}

interface TransactionListResponse {
    items: Array<Omit<Transaction, 'date'> & { date: string }>;
    hasMore: boolean;
    nextCursorDate: string | null;
    nextCursorId: string | null;
}

@Service()
export class TransactionService {
    private http = inject(HttpClient);

    getTransactions(filters: TransactionFilters = {}) : Observable<Transaction[]> {

        let params = new HttpParams();

        if (filters.year !== undefined && filters.year !== null) {
            params = params.set('year', String(filters.year));
        }

        if (filters.month !== undefined && filters.month !== null) {
            params = params.set('month', String(filters.month));
        }

        if (filters.accountId) {
            params = params.set('accountId', filters.accountId);
        }

        if (filters.description) {
            params = params.set('description', filters.description);
        }

        if (filters.cursorDate) {
            params = params.set('cursorDate', filters.cursorDate);
        }

        if (filters.cursorId) {
            params = params.set('cursorId', filters.cursorId);
        }

        if (filters.pageSize !== undefined && filters.pageSize !== null) {
            params = params.set('pageSize', String(filters.pageSize));
        }

        return this.http.get<TransactionListResponse>('/api/transactions/', { params }).pipe(
            map((response) => response.items.map((transaction) => ({
                ...transaction,
                date: new Date(transaction.date),
            }))),
        );
    }

    getTransactionById(transactionId: string) : Observable<Transaction> {
        return this.http.get<Transaction>(`/api/transactions/${transactionId}`);
    }

    createTransaction(transaction: Transaction) : Observable<Transaction> {
        return this.http.post<Transaction>('/api/transactions', transaction);
    }

    updateTransaction(transaction: Transaction) : Observable<Transaction> {
        return this.http.put<Transaction>(`/api/transactions/${transaction.id}`, transaction);
    }

    deleteTransaction(transactionId: string) : Observable<void> {
        return this.http.delete<void>(`/api/transactions/${transactionId}`);
    }
}
