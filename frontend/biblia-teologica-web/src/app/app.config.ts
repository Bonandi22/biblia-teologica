import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideEnvironmentInitializer,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { routes } from './app.routes';
import { withCredentialsInterceptor } from './core/interceptors/with-credentials.interceptor';
import { csrfInterceptor } from './core/interceptors/csrf.interceptor';
import { refreshInterceptor } from './core/interceptors/refresh.interceptor';
import { AuthService } from './core/auth/auth.service';
import { rolarAoTopoAoTrocarDePagina } from './core/navegacao/rolagem';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideEnvironmentInitializer(rolarAoTopoAoTrocarDePagina),
    provideHttpClient(
      withFetch(),
      withInterceptors([withCredentialsInterceptor, csrfInterceptor, refreshInterceptor]),
    ),
    provideAppInitializer(() => firstValueFrom(inject(AuthService).carregarUsuario())),
  ],
};
