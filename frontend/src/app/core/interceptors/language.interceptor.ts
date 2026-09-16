import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';


export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  const translocoService = inject(TranslocoService);
  
  return next(req.clone({
    setHeaders: { 'Accept-Language': translocoService.getActiveLang() }
  }));
};
