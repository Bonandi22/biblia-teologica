import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, shareReplay, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from '../auth/auth.store';

const AUTH_BASE = `${environment.apiBaseUrl}/auth/`;

// Partilhado entre invocações: um único refresh serve todos os pedidos que
// apanharam 401 em paralelo.
let refreshEmCurso: Observable<unknown> | null = null;

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
  const http = inject(HttpClient);
  const router = inject(Router);
  const store = inject(AuthStore);

  const ehAuth = req.url.startsWith(AUTH_BASE);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401 || ehAuth) {
        return throwError(() => err);
      }

      // `http.post` volta a passar pela cadeia de interceptors — o csrfInterceptor
      // injeta o header `X-CSRF-Token` exigido por `POST /api/auth/refresh`.
      refreshEmCurso ??= http
        .post(`${AUTH_BASE}refresh`, {})
        .pipe(shareReplay({ bufferSize: 1, refCount: false }));

      return refreshEmCurso.pipe(
        switchMap(() => {
          refreshEmCurso = null;
          return next(req);
        }),
        catchError((e: unknown) => {
          refreshEmCurso = null;
          store.limpar();
          void router.navigate(['/entrar']);
          return throwError(() => e);
        }),
      );
    }),
  );
};
