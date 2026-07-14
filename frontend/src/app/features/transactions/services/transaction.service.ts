import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Transaction } from '../models/transaction.model';
import { map, Observable } from 'rxjs';

export interface TransactionFilters {
    dateFrom?: string | null;
    dateTo?: string | null;
    accountId?: string | null;
    description?: string | null;
    cursorDate?: string | null;
    cursorId?: string | null;
    pageSize?: number | null;
}

interface TransactionDto extends Omit<Transaction, 'date'> {
    date: string;
}

interface GetTransactionsPageResponseDto {
    items: TransactionDto[];
    hasMore: boolean;
    nextCursorDate: string | null;
    nextCursorId: string | null;
}

export interface TransactionPage {
    items: Transaction[];
    hasMore: boolean;
    nextCursorDate: string | null;
    nextCursorId: string | null;
}

export interface SaveTransactionDto {
    date: string;
    description: string;
    amount: number;
    type: 'Income' | 'Expense' | 'Transfer';
    accountId: string;
}

export interface SaveTransferCommand {
    date: string;
    description: string;
    amount: number;
    fromAccountId: string;
    toAccountId: string;
}

export interface SaveTransferResult {
    fromTransaction: Transaction;
    toTransaction: Transaction;
}

@Service()
export class TransactionService {
    private http = inject(HttpClient);

    getTransactionsPage(filters: TransactionFilters = {}) : Observable<TransactionPage> {

        let params = new HttpParams();

        if (filters.dateFrom) {
            params = params.set('dateFrom', filters.dateFrom);
        }

        if (filters.dateTo) {
            params = params.set('dateTo', filters.dateTo);
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

        return this.http.get<GetTransactionsPageResponseDto>('/api/transactions/', { params }).pipe(
            map((responseDto) => this.mapGetTransactionsPageResponseDtoToTransactionPage(responseDto)),
        );
    }

    getTransactions(filters: TransactionFilters = {}) : Observable<Transaction[]> {
        return this.getTransactionsPage(filters).pipe(
            map((response) => response.items),
        );
    }

    getTransactionById(transactionId: string) : Observable<Transaction> {
        return this.http.get<Transaction>(`/api/transactions/${transactionId}`);
    }

    createTransaction(command: SaveTransactionDto) : Observable<Transaction> {
        return this.http.post<Transaction>('/api/transactions', command);
    }

    updateTransaction(transactionId: string, command: SaveTransactionDto) : Observable<Transaction> {
        return this.http.put<Transaction>(`/api/transactions/${transactionId}`, command);
    }

    deleteTransaction(transactionId: string) : Observable<void> {
        return this.http.delete<void>(`/api/transactions/${transactionId}`);
    }

    createTransfer(command: SaveTransferCommand) : Observable<SaveTransferResult> {
        return this.http.post<SaveTransferResult>('/api/transactions/transfer', command);
    }

    updateTransfer(transactionId: string, command: SaveTransferCommand) : Observable<SaveTransferResult> {
        return this.http.put<SaveTransferResult>(`/api/transactions/transfer/${transactionId}`, command);
    }

    private mapGetTransactionsPageResponseDtoToTransactionPage(responseDto: GetTransactionsPageResponseDto): TransactionPage {
        return {
            items: responseDto.items.map((transactionDto) => this.mapTransactionDtoToTransaction(transactionDto)),
            hasMore: responseDto.hasMore,
            nextCursorDate: responseDto.nextCursorDate,
            nextCursorId: responseDto.nextCursorId,
        };
    }

    private mapTransactionDtoToTransaction(transactionDto: TransactionDto): Transaction {
        return {
            ...transactionDto,
            date: new Date(transactionDto.date),
        };
    }
}
