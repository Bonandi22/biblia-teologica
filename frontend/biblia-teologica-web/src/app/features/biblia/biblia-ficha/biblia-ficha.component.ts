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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription, map } from 'rxjs';
import type { LivroDetail } from '../../../core/models/livro.model';
import { LivroService } from '../../../core/services/livro.service';
import { LivroStore } from '../../../core/services/livro.store';
import { LivroMolduraComponent } from '../livro-moldura/livro-moldura.component';

@Component({
  selector: 'bt-biblia-ficha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, LivroMolduraComponent],
  templateUrl: './biblia-ficha.component.html',
  styleUrl: './biblia-ficha.component.scss',
})
export class BibliaFichaComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(LivroService);
  private readonly store = inject(LivroStore);
  private readonly destroyRef = inject(DestroyRef);
  private requisicao: Subscription | null = null;

  readonly slug = toSignal(this.route.paramMap.pipe(map((p) => p.get('slug') ?? '')), {
    initialValue: '',
  });

  readonly livro = signal<LivroDetail | null>(null);
  readonly vizinhos = computed(() => this.store.vizinhos(this.slug()));

  constructor() {
    this.store.carregar();
    // Recarrega a ficha sempre que o slug da rota muda.
    effect(() => {
      const slug = this.slug();
      untracked(() => this.carregar(slug));
    });
  }

  private carregar(slug: string): void {
    if (!slug) return;
    this.requisicao?.unsubscribe();
    this.livro.set(null);
    this.requisicao = this.service
      .obterPorSlug(slug)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (l) => this.livro.set(l),
        error: () => {
          this.router.navigate(['/biblia']).catch(() => {});
        },
      });
  }
}
