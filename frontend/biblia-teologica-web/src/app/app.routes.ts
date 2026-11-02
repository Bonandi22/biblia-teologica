import { Routes } from '@angular/router';
import { environment } from '../environments/environment';
import { authGuard } from './core/auth/guards/auth.guard';
import { EmBreveComponent } from './features/institucional/em-breve.component';

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
    path: 'estudos',
    component: EmBreveComponent,
    data: {
      titulo: 'Estudos',
      descricao:
        'Artigos longos sobre teologia, história da igreja e vida cristã. Esta secção está a ser preparada.',
    },
  },
  {
    path: 'noticias',
    component: EmBreveComponent,
    data: {
      titulo: 'Notícias',
      descricao:
        'Notícias da igreja e do mundo, lidas à luz da Bíblia. Esta secção está a ser preparada.',
    },
  },
  {
    path: 'curiosidades',
    component: EmBreveComponent,
    data: {
      titulo: 'Curiosidades',
      descricao:
        'Pequenas descobertas sobre o texto bíblico e o seu contexto. Esta secção está a ser preparada.',
    },
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
  ...(environment.production
    ? []
    : [
        {
          path: 'ui',
          loadComponent: () =>
            import('./features/ui-provas/ui-provas.component').then((m) => m.UiProvasComponent),
        },
      ]),
  { path: '**', redirectTo: '' },
];
