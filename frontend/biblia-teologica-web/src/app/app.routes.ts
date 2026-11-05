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
    loadChildren: () => import('./features/estudos/estudos.routes').then((m) => m.estudosRoutes),
  },
  {
    path: 'noticias',
    component: EmBreveComponent,
    data: {
      titulo: 'Notícias',
      descricao: 'Esta seção ainda não tem conteúdo.',
    },
  },
  {
    path: 'curiosidades',
    component: EmBreveComponent,
    data: {
      titulo: 'Curiosidades',
      descricao: 'Esta seção ainda não tem conteúdo.',
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
    path: 'esqueci-senha',
    loadComponent: () =>
      import('./features/auth/esqueci-senha/esqueci-senha.component').then(
        (m) => m.EsqueciSenhaComponent,
      ),
  },
  {
    path: 'redefinir-senha',
    loadComponent: () =>
      import('./features/auth/redefinir-senha/redefinir-senha.component').then(
        (m) => m.RedefinirSenhaComponent,
      ),
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
