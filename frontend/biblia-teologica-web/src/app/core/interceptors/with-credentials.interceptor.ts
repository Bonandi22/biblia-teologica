import { HttpInterceptorFn } from '@angular/common/http';

export const withCredentialsInterceptor: HttpInterceptorFn = (req, next) =>
  req.url.startsWith('/api') ? next(req.clone({ withCredentials: true })) : next(req);
