import { Routes } from '@angular/router';
import { BibliaListaComponent } from './biblia-lista/biblia-lista.component';
import { livroMatcher } from './livro-pagina/livro.matcher';

export const bibliaRoutes: Routes = [
  { path: '', pathMatch: 'full', component: BibliaListaComponent },
  {
    // `:slug` e `:slug/:aba` na mesma rota: trocar de aba reaproveita o componente.
    matcher: livroMatcher,
    loadComponent: () =>
      import('./livro-pagina/livro-pagina.component').then((m) => m.LivroPaginaComponent),
  },
];
