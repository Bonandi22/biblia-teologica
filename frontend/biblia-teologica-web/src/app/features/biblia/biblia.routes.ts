import { Routes } from '@angular/router';
import { BibliaListaComponent } from './biblia-lista/biblia-lista.component';

export const bibliaRoutes: Routes = [
  {
    path: '',
    component: BibliaListaComponent,
    children: [
      {
        path: ':slug',
        loadComponent: () =>
          import('./biblia-ficha/biblia-ficha.component').then((m) => m.BibliaFichaComponent),
      },
    ],
  },
];
