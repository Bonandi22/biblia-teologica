import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { Estudo, EstudoResumo, LivroPanorama } from '../models/conteudo.model';

@Injectable({ providedIn: 'root' })
export class ConteudoService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  obterLivro(slug: string): Observable<LivroPanorama> {
    return this.http.get<LivroPanorama>(`${this.base}/panoramas/livros/${slug}`);
  }

  listarLivrosComPanorama(): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/panoramas/livros`);
  }

  listarEstudos(): Observable<EstudoResumo[]> {
    return this.http.get<EstudoResumo[]>(`${this.base}/estudos`);
  }

  obterEstudo(slug: string): Observable<Estudo> {
    return this.http.get<Estudo>(`${this.base}/estudos/${slug}`);
  }
}
