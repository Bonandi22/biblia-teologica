import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from './auth.store';
import type { DesafioLogin, Usuario } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthStore);
  private readonly base = `${environment.apiBaseUrl}/auth`;

  login(email: string, senha: string): Observable<DesafioLogin> {
    return this.http.post<DesafioLogin>(`${this.base}/login`, { email, senha });
  }

  verificar2fa(tokenDesafio: string, codigo: string): Observable<Usuario> {
    return this.http
      .post<{ usuario: Usuario }>(`${this.base}/login/2fa`, { tokenDesafio, codigo })
      .pipe(
        map((r) => r.usuario),
        tap((u) => this.store.definir(u)),
      );
  }

  registar(dto: { email: string; senha: string; nomeExibicao: string }): Observable<void> {
    return this.http.post<void>(`${this.base}/registar`, dto);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.base}/logout`, {}).pipe(tap(() => this.store.limpar()));
  }

  carregarUsuario(): Observable<Usuario | null> {
    return this.http.get<Usuario>(`${this.base}/eu`).pipe(
      tap((u) => this.store.definir(u)),
      catchError(() => {
        this.store.limpar();
        return of(null);
      }),
    );
  }
}
