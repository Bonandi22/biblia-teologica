import { Routes } from '@angular/router';
import { authGuard } from './core/auth/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'biblia',
    loadChildren: () => import('./features/biblia/biblia.routes').then((m) => m.bibliaRoutes),
  },
  {
    path: 'entrar',
    loadComponent: () =>
      import('./features/auth/entrar/entrar.component').then((m) => m.EntrarComponent),
  },
  {
    path: 'registar',
    loadComponent: () =>
      import('./features/auth/registar/registar.component').then((m) => m.RegistarComponent),
  },
  {
    path: 'conta',
    canActivate: [authGuard],
    loadChildren: () => import('./features/conta/conta.routes').then((m) => m.contaRoutes),
  },
  // `admin` entra no M2b (features/admin ainda não existe).
  { path: '**', redirectTo: '' },
];
