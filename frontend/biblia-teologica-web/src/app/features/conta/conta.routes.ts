import { Routes } from '@angular/router';

export const contaRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./perfil/perfil.component').then((m) => m.PerfilComponent),
  },
  {
    path: 'trocar-senha',
    loadComponent: () =>
      import('./trocar-senha/trocar-senha.component').then((m) => m.TrocarSenhaComponent),
  },
];
