import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription, map } from 'rxjs';
import type { Bloco, LivroPanorama } from '../../../core/models/conteudo.model';
import { ConteudoService } from '../../../core/services/conteudo.service';
import { LivroStore } from '../../../core/services/livro.store';
import { BlocoConteudoComponent } from '../../../shared/ui/bloco-conteudo/bloco-conteudo.component';
import { LinhaEstudoComponent } from '../../../shared/ui/linha-estudo/linha-estudo.component';
import { LivroMolduraComponent } from '../livro-moldura/livro-moldura.component';
import { BibliaFichaComponent } from '../biblia-ficha/biblia-ficha.component';
import { CitacoesComponent } from './citacoes/citacoes.component';
import { ApontamentosComponent } from './apontamentos/apontamentos.component';

type Estado = 'carregando' | 'panorama' | 'ficha' | 'erro';
type ChaveAba = 'panorama' | 'contexto' | 'mundo' | 'profecias' | 'promessas' | 'citacoes';

interface Aba {
  chave: ChaveAba;
  rotulo: string;
}

const ABAS_BASE: Aba[] = [
  { chave: 'panorama', rotulo: 'Panorama' },
  { chave: 'contexto', rotulo: 'Contexto' },
  { chave: 'mundo', rotulo: 'Mundo' },
];
const ABA_PROFECIAS: Aba = { chave: 'profecias', rotulo: 'Profecias' };
const ABA_PROMESSAS: Aba = { chave: 'promessas', rotulo: 'Promessas' };
const ABA_CITACOES: Aba = { chave: 'citacoes', rotulo: 'Citações' };

@Component({
  selector: 'bt-livro-pagina',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    BibliaFichaComponent,
    BlocoConteudoComponent,
    LinhaEstudoComponent,
    LivroMolduraComponent,
    ApontamentosComponent,
    CitacoesComponent,
  ],
  templateUrl: './livro-pagina.component.html',
  styleUrl: './livro-pagina.component.scss',
})
export class LivroPaginaComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly conteudo = inject(ConteudoService);
  private readonly store = inject(LivroStore);
  private readonly titulo = inject(Title);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly slug = toSignal(this.route.paramMap.pipe(map((p) => p.get('slug') ?? '')), {
    initialValue: '',
  });
  private readonly abaDaRota = toSignal(this.route.paramMap.pipe(map((p) => p.get('aba'))), {
    initialValue: null,
  });

  protected readonly estado = signal<Estado>('carregando');
  protected readonly livro = signal<LivroPanorama | null>(null);
  private readonly tentativa = signal(0);
  private requisicao: Subscription | null = null;

  protected readonly nome = computed(
    () => this.store.livros().find((l) => l.slug === this.slug())?.nome ?? this.slug(),
  );

  protected readonly abas = computed<Aba[]>(() => {
    const l = this.livro();
    return [
      ...ABAS_BASE,
      ...(l && l.profecias.length > 0 ? [ABA_PROFECIAS] : []),
      ...(l && l.promessas.length > 0 ? [ABA_PROMESSAS] : []),
      ...(l && l.citacoes.length > 0 ? [ABA_CITACOES] : []),
    ];
  });

  protected readonly primeiroPerfilComReferencia = computed(
    () => this.livro()?.perfil.findIndex((p) => p.referencias.length > 0) ?? -1,
  );

  /** Aba mostrada: sem segmento de aba na URL vale o panorama. */
  protected readonly ativa = computed<ChaveAba>(() => (this.abaDaRota() ?? 'panorama') as ChaveAba);

  protected readonly rotuloAtivo = computed(
    () => this.abas().find((a) => a.chave === this.ativa())?.rotulo ?? '',
  );

  /** Nome só quando o fichário já carregou; nunca o slug (o título não pode vazá-lo). */
  private readonly nomeCarregado = computed(
    () => this.store.livros().find((l) => l.slug === this.slug())?.nome ?? null,
  );

  constructor() {
    this.store.carregar();

    effect(() => {
      const slug = this.slug();
      this.tentativa();
      untracked(() => this.carregar(slug));
    });

    effect(() => {
      if (this.estado() !== 'panorama') return;
      const aba = this.abaDaRota();
      if (aba !== null && !this.abas().some((a) => a.chave === aba)) {
        const slug = untracked(() => this.slug());
        this.router.navigate(['/biblia', slug, 'panorama'], { replaceUrl: true }).catch(() => {});
      }
    });

    effect(() => {
      const estado = this.estado();
      const nome = this.nomeCarregado();
      if (estado === 'erro') {
        this.titulo.setTitle('Erro ao carregar o livro | Bíblia Teológica');
      } else if (estado === 'ficha' && nome) {
        this.titulo.setTitle(`${nome} | Bíblia Teológica`);
      } else if (estado === 'panorama' && nome && this.rotuloAtivo()) {
        this.titulo.setTitle(`${nome}: ${this.rotuloAtivo()} | Bíblia Teológica`);
      }
    });
  }

  protected tentarNovamente(): void {
    this.tentativa.update((n) => n + 1);
  }

  protected bloco(chave: ChaveAba): Bloco | null {
    const l = this.livro();
    if (!l) return null;
    return chave === 'panorama' || chave === 'contexto' || chave === 'mundo' ? l[chave] : null;
  }

  private carregar(slug: string): void {
    if (!slug) return;
    this.requisicao?.unsubscribe();
    this.estado.set('carregando');
    this.livro.set(null);
    this.requisicao = this.conteudo
      .obterLivro(slug)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (l) => {
          this.livro.set(l);
          this.estado.set('panorama');
        },
        error: (e: unknown) => {
          // 404: o livro ainda não tem panorama; segue a ficha de sempre.
          this.estado.set(e instanceof HttpErrorResponse && e.status === 404 ? 'ficha' : 'erro');
        },
      });
  }
}
