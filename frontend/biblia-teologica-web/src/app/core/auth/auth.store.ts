import { Injectable, computed, signal } from '@angular/core';
import type { Usuario } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly _usuario = signal<Usuario | null>(null);
  private readonly _carregado = signal(false);

  readonly usuario = this._usuario.asReadonly();
  readonly carregado = this._carregado.asReadonly();

  readonly estaAutenticado = computed(() => this._usuario() !== null);
  readonly ehAdmin = computed(() => this._usuario()?.roles.includes('Admin') ?? false);
  readonly deveTrocarSenha = computed(() => this._usuario()?.deveTrocarSenha ?? false);

  definir(u: Usuario): void {
    this._usuario.set(u);
    this._carregado.set(true);
  }

  limpar(): void {
    this._usuario.set(null);
    this._carregado.set(true);
  }

  marcarCarregado(): void {
    this._carregado.set(true);
  }
}
