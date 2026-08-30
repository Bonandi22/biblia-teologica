import { HttpInterceptorFn } from '@angular/common/http';
import { lerCookie } from './lercookie';

const INSEGUROS = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

export const csrfInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('/api') && INSEGUROS.has(req.method)) {
    const csrf = lerCookie('bt_csrf');
    if (csrf) return next(req.clone({ setHeaders: { 'X-CSRF-Token': csrf } }));
  }
  return next(req);
};
