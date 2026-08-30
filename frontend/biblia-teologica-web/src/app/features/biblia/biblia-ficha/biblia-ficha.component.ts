import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { BadgeSecaoComponent } from '../../../shared/components/badge-secao/badge-secao.component';
import { CampoFichaComponent } from '../../../shared/components/campo-ficha/campo-ficha.component';
import { LivroService } from '../../../core/services/livro.service';
import { LivroStore } from '../../../core/services/livro.store';
import type { LivroDetail } from '../../../core/models/livro.model';

@Component({
  selector: 'bt-biblia-ficha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, BadgeSecaoComponent, CampoFichaComponent],
  templateUrl: './biblia-ficha.component.html',
  styleUrl: './biblia-ficha.component.scss',
})
export class BibliaFichaComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(LivroService);
  private readonly store = inject(LivroStore);

  readonly slug = toSignal(this.route.paramMap.pipe(map((p) => p.get('slug') ?? '')), {
    initialValue: '',
  });

  readonly livro = signal<LivroDetail | null>(null);
  readonly vizinhos = computed(() => this.store.vizinhos(this.slug()));

  constructor() {
    this.store.carregar();
    // recarrega a ficha sempre que o slug da rota muda
    effect(() => this.carregar(this.slug()));
  }

  private carregar(slug: string): void {
    if (!slug) return;
    this.service.obterPorSlug(slug).subscribe({
      next: (l) => this.livro.set(l),
      error: () => {
        this.router.navigate(['/biblia']).catch(() => {});
      },
    });
  }
}
