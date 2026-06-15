import { Routes } from '@angular/router';
import { authGuard, publicGuard } from '@core/guards/auth.guard';
import { AuthLayout } from '@layouts/auth-layout/auth-layout';
import { MainLayout } from '@layouts/main-layout/main-layout';

export const routes: Routes = [
    // Public routes with AuthLayout (without navbar, sidebar, etc.)
    {
        path: 'auth', 
        component: AuthLayout, 
        canActivate: [publicGuard],
        children: [
            { path: 'login', loadComponent: () => import('@features/auth/login/login').then(m => m.Login) },
            { path: 'setup', loadComponent: () => import('@features/auth/setup/setup').then(m => m.Setup) },
            { path: '', redirectTo: 'login', pathMatch: 'full' }
        ]
    }, 
    // Protected routes (with navigation layout, navbar, sidebar, etc.)
    {
        path: '', 
        component: MainLayout, 
        canActivate: [authGuard],
        children: [
            { path: 'hello', loadComponent: () => import('@features/hello/hello').then(m => m.Hello) }, 
            { path: '', redirectTo: 'hello', pathMatch: 'full'}
        ]
        
    }, 
    // Redirect not found routes to root (which redirects to login if not authenticated)
    { path: '**', redirectTo: '', pathMatch: 'full' },
];
