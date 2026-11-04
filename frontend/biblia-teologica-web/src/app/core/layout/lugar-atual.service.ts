import { Injectable, signal } from '@angular/core';

/** Texto central do título corrente, como "Isaías 1–66". Quem define a página também limpa. */
@Injectable({ providedIn: 'root' })
export class LugarAtualService {
  private readonly _texto = signal('');
  readonly texto = this._texto.asReadonly();

  definir(texto: string): void {
    this._texto.set(texto);
  }

  limpar(): void {
    this._texto.set('');
  }
}
