import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import {
    GetDailyBalanceResult,
    GetDailyCashflowResult,
    GetMonthlyBalanceResult,
    GetMonthlyCashflowResult,
    SummaryQueryParams,
} from '../models/summary.model';

@Service()
export class SummaryService {
    private http = inject(HttpClient);

    getDailyBalance(params: SummaryQueryParams = {}): Observable<GetDailyBalanceResult> {
        return this.http.get<GetDailyBalanceResult>('/api/summary/daily-balance', {
            params: this.buildParams(params),
        });
    }

    getDailyCashflow(params: SummaryQueryParams = {}): Observable<GetDailyCashflowResult> {
        return this.http.get<GetDailyCashflowResult>('/api/summary/daily-cashflow', {
            params: this.buildParams(params),
        });
    }

    getMonthlyBalance(params: SummaryQueryParams = {}): Observable<GetMonthlyBalanceResult> {
        return this.http.get<GetMonthlyBalanceResult>('/api/summary/monthly-balance', {
            params: this.buildParams(params),
        });
    }

    getMonthlyCashflow(params: SummaryQueryParams = {}): Observable<GetMonthlyCashflowResult> {
        return this.http.get<GetMonthlyCashflowResult>('/api/summary/monthly-cashflow', {
            params: this.buildParams(params),
        });
    }

    private buildParams({ accountId, accountType, month, year, currency }: SummaryQueryParams): HttpParams {
        let params = new HttpParams();

        if (accountId) params = params.set('accountId', accountId);
        if (!accountId && accountType) params = params.set('accountType', accountType);
        if (month != null) params = params.set('month', month);
        if (year != null) params = params.set('year', year);
        if (currency) params = params.set('currency', currency);

        return params;
    }
}
