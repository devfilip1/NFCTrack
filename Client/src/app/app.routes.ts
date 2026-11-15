import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
    { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
    { path: 'register', loadComponent: () => import('./features/auth/register/register').then(m => m.Register) },
    {
        path: '', canActivate: [authGuard], loadComponent: () => import('./core/layout/shell/shell').then(m => m.Shell), children: [
            { path: 'links', loadComponent: () => import('./features/links/link-list/link-list').then(m => m.LinkList) },
            { path: '', pathMatch: 'full', redirectTo: 'links' }
        ]
    },
    { path: '**', redirectTo: '' },
];
