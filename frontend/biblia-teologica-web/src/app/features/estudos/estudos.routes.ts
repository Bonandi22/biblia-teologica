import { Routes } from '@angular/router';

export const estudosRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'Estudos | Bíblia Teológica',
    loadComponent: () =>
      import('./estudos-lista/estudos-lista.component').then((m) => m.EstudosListaComponent),
  },
  {
    path: ':slug',
    loadComponent: () =>
      import('./estudo-pagina/estudo-pagina.component').then((m) => m.EstudoPaginaComponent),
  },
];
