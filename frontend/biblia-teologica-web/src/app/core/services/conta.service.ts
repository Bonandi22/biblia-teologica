import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from '../auth/auth.store';
import type { Usuario } from '../auth/auth.models';

export interface Perfil {
  email: string;
  nomeExibicao: string;
  roles: string[];
  criadoEm: string;
  telefone: string | null;
}

@Injectable({ providedIn: 'root' })
export class ContaService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthStore);
  private readonly base = `${environment.apiBaseUrl}/conta`;

  obter(): Observable<Perfil> {
    return this.http.get<Perfil>(this.base);
  }

  atualizarNome(nomeExibicao: string): Observable<void> {
    return this.http.put<void>(this.base, { nomeExibicao });
  }

  trocarEmail(dto: { novoEmail: string; senha: string }): Observable<void> {
    return this.http.put<void>(`${this.base}/email`, dto);
  }

  atualizarPerfil(dto: { nomeExibicao: string; telefone: string | null }): Observable<void> {
    return this.http.put<void>(this.base, dto);
  }

  solicitarTrocaEmail(dto: { novoEmail: string; senha: string }): Observable<void> {
    return this.http.post<void>(`${this.base}/email/solicitar`, dto);
  }

  confirmarTrocaEmail(dto: { novoEmail: string; codigo: string }): Observable<Usuario> {
    return this.http.post<{ usuario: Usuario }>(`${this.base}/email/confirmar`, dto).pipe(
      map((r) => r.usuario),
      tap((u) => this.store.definir(u)),
    );
  }

  apagar(senha: string): Observable<void> {
    return this.http.delete<void>(this.base, { body: { senha } });
  }

  trocarSenha(dto: { senhaAtual: string; novaSenha: string }): Observable<Usuario> {
    return this.http.post<{ usuario: Usuario }>(`${this.base}/senha`, dto).pipe(
      map((r) => r.usuario),
      tap((u) => this.store.definir(u)),
    );
  }
}
