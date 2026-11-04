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
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription, map } from 'rxjs';
import { LugarAtualService } from '../../../core/layout/lugar-atual.service';
import type { Estudo } from '../../../core/models/conteudo.model';
import { ConteudoService } from '../../../core/services/conteudo.service';
import { LivroStore } from '../../../core/services/livro.store';
import { LinhaEstudoComponent } from '../../../shared/ui/linha-estudo/linha-estudo.component';
import { blocosMarkdown } from '../../../shared/ui/markdown/markdown';
import { MarkdownComponent } from '../../../shared/ui/markdown/markdown.component';
import { notasDoTexto } from '../../../shared/ui/texto-notas/notas-do-texto';

type Estado = 'carregando' | 'pronto' | 'naoEncontrado' | 'erro';

@Component({
  selector: 'bt-estudo-pagina',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, MarkdownComponent, LinhaEstudoComponent],
  templateUrl: './estudo-pagina.component.html',
  styleUrl: './estudo-pagina.component.scss',
})
export class EstudoPaginaComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly conteudo = inject(ConteudoService);
  private readonly store = inject(LivroStore);
  private readonly titulo = inject(Title);
  private readonly destroyRef = inject(DestroyRef);

  private readonly slug = toSignal(this.route.paramMap.pipe(map((p) => p.get('slug') ?? '')), {
    initialValue: '',
  });

  protected readonly estado = signal<Estado>('carregando');
  protected readonly estudo = signal<Estudo | null>(null);
  private readonly tentativa = signal(0);

  /** Cada bloco do texto com as notas que chama; cada nota só aparece uma vez, porque o id é único. */
  protected readonly linhas = computed(() => {
    const e = this.estudo();
    const usadas = new Set<number>();
    let comTitulo = true;
    const blocos = blocosMarkdown(e?.corpoMarkdown ?? '').map((md) => {
      const notas = notasDoTexto(md, e?.notas ?? []).filter((n) => !usadas.has(n.numero));
      notas.forEach((n) => usadas.add(n.numero));
      const titulo = comTitulo && notas.length > 0;
      if (titulo) comTitulo = false;
      return { md, notas, comTitulo: titulo };
    });
    return { blocos, restantes: (e?.notas ?? []).filter((n) => !usadas.has(n.numero)) };
  });

  private requisicao: Subscription | null = null;

  constructor() {
    this.store.carregar();
    const lugar = inject(LugarAtualService);
    lugar.definir('Estudo');
    this.destroyRef.onDestroy(() => lugar.limpar());

    effect(() => {
      const slug = this.slug();
      this.tentativa();
      untracked(() => this.carregar(slug));
    });

    effect(() => {
      const e = this.estudo();
      const estado = this.estado();
      if (estado === 'pronto' && e) this.titulo.setTitle(`${e.titulo} | Bíblia Teológica`);
      else if (estado === 'naoEncontrado') this.titulo.setTitle('Estudo não encontrado | Bíblia Teológica');
      else if (estado === 'erro') this.titulo.setTitle('Erro ao carregar o estudo | Bíblia Teológica');
    });
  }

  protected tentarNovamente(): void {
    this.tentativa.update((n) => n + 1);
  }

  /** Nome do livro quando o fichário já carregou; senão o próprio slug. */
  protected nomeLivro(slug: string): string {
    return this.store.livros().find((l) => l.slug === slug)?.nome ?? slug;
  }

  private carregar(slug: string): void {
    if (!slug) return;
    this.requisicao?.unsubscribe();
    this.estado.set('carregando');
    this.estudo.set(null);
    this.requisicao = this.conteudo
      .obterEstudo(slug)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (e) => {
          this.estudo.set(e);
          this.estado.set('pronto');
        },
        error: (e: unknown) =>
          this.estado.set(e instanceof HttpErrorResponse && e.status === 404 ? 'naoEncontrado' : 'erro'),
      });
  }
}
