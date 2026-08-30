import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { LivroDetail, LivroListItem } from '../models/livro.model';

@Injectable({ providedIn: 'root' })
export class LivroService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/livros`;

  listar(): Observable<LivroListItem[]> {
    return this.http.get<LivroListItem[]>(this.base);
  }

  obterPorSlug(slug: string): Observable<LivroDetail> {
    return this.http.get<LivroDetail>(`${this.base}/${slug}`);
  }
}
