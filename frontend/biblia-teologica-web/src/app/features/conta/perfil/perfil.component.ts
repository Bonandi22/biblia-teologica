import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AlertaComponent } from '../../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../../shared/ui/botao/botao.component';
import { BtInputDirective } from '../../../shared/ui/bt-input.directive';
import { CampoComponent } from '../../../shared/ui/campo/campo.component';
import { erroCampo } from '../../../shared/ui/campo/erro-campo';
import { CampoSenhaComponent } from '../../../shared/ui/campo-senha/campo-senha.component';
import { ContaService, Perfil } from '../../../core/services/conta.service';
import { AuthService } from '../../../core/auth/auth.service';

interface Aviso {
  tipo: 'erro' | 'sucesso';
  texto: string;
}

@Component({
  selector: 'bt-conta-perfil',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    AlertaComponent,
    BotaoComponent,
    BtInputDirective,
    CampoComponent,
    CampoSenhaComponent,
  ],
  templateUrl: './perfil.component.html',
  styleUrl: './perfil.component.scss',
})
export class PerfilComponent {
  private readonly fb = inject(FormBuilder);
  private readonly conta = inject(ContaService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly perfil = signal<Perfil | null>(null);
  readonly falhouAoCarregar = signal(false);
  readonly avisoNome = signal<Aviso | null>(null);
  readonly avisoEmail = signal<Aviso | null>(null);
  readonly erroAoSair = signal<string | null>(null);
  readonly aEnviarNome = signal(false);
  readonly aEnviarEmail = signal(false);
  readonly aSair = signal(false);

  readonly formNome = this.fb.nonNullable.group({
    nomeExibicao: ['', [Validators.required, Validators.maxLength(80)]],
  });
  readonly formEmail = this.fb.nonNullable.group({
    novoEmail: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });

  constructor() {
    inject(Title).setTitle('Minha conta | Bíblia Teológica');
    this.carregar();
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  carregar(): void {
    this.falhouAoCarregar.set(false);
    this.conta
      .obter()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (p) => {
          this.perfil.set(p);
          this.formNome.patchValue({ nomeExibicao: p.nomeExibicao });
        },
        error: () => this.falhouAoCarregar.set(true),
      });
  }

  guardarNome(): void {
    this.formNome.markAllAsTouched();
    if (this.formNome.invalid) return;
    this.avisoNome.set(null);
    this.aEnviarNome.set(true);
    this.conta
      .atualizarNome(this.formNome.getRawValue().nomeExibicao)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.avisoNome.set({ tipo: 'sucesso', texto: 'Nome salvo.' });
          this.aEnviarNome.set(false);
        },
        error: () => {
          this.avisoNome.set({ tipo: 'erro', texto: 'Não foi possível salvar o nome. Tente de novo.' });
          this.aEnviarNome.set(false);
        },
      });
  }

  guardarEmail(): void {
    this.formEmail.markAllAsTouched();
    if (this.formEmail.invalid) return;
    this.avisoEmail.set(null);
    this.aEnviarEmail.set(true);
    const dados = this.formEmail.getRawValue();
    this.conta
      .trocarEmail(dados)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.perfil.update((p) => p && { ...p, email: dados.novoEmail });
          this.formEmail.reset();
          this.avisoEmail.set({ tipo: 'sucesso', texto: 'E-mail alterado. Use-o no próximo login.' });
          this.aEnviarEmail.set(false);
        },
        error: (e) => {
          this.avisoEmail.set({ tipo: 'erro', texto: e.error?.detail ?? 'Não foi possível alterar o e-mail.' });
          this.aEnviarEmail.set(false);
        },
      });
  }

  sair(): void {
    this.erroAoSair.set(null);
    this.aSair.set(true);
    this.auth
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => void this.router.navigate(['/']),
        // A sessão só termina no servidor: limpar aqui faria parecer que saiu com os cookies ainda válidos.
        error: () => {
          this.erroAoSair.set('Não foi possível sair. Tente de novo.');
          this.aSair.set(false);
        },
      });
  }
}
