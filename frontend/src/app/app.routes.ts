import { Routes } from '@angular/router';
import { authGuard, visitanteGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [visitanteGuard],
    loadComponent: () => import('./pages/login/login').then((m) => m.Login),
  },
  {
    path: 'cadastro',
    canActivate: [visitanteGuard],
    loadComponent: () => import('./pages/register/register').then((m) => m.Register),
  },
  {
    path: 'painel',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
  },
  {
    path: 'url/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/stats/stats').then((m) => m.Stats),
  },
  { path: '', pathMatch: 'full', redirectTo: 'painel' },
  { path: '**', redirectTo: 'painel' },
];
