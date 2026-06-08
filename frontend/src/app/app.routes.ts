import { Routes } from '@angular/router';
import { AuthLayout } from '@layouts/auth-layout/auth-layout';
import { MainLayout } from '@layouts/main-layout/main-layout';

export const routes: Routes = [
    {
        path: 'auth', 
        component: AuthLayout, 
        children: [
            { path: 'login', loadComponent: () => import('@features/auth/login/login').then(m => m.Login) },
            { path: '', redirectTo: 'login', pathMatch: 'full' }
        ]
    }, 
    {
        path: '', 
        component: MainLayout, 
        children: [
            { path: 'aaa', loadComponent: () => import('@features/auth/login/login').then(m => m.Login) }, 
            { path: '', redirectTo: 'aaa', pathMatch: 'full'}
        ]
        
    }
];
