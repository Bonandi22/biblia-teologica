import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'biblia' },
  {
    path: 'biblia',
    loadChildren: () => import('./features/biblia/biblia.routes').then((m) => m.bibliaRoutes),
  },
  { path: '**', redirectTo: 'biblia' },
];
