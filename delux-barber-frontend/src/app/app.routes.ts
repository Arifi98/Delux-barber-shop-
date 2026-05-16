import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  { path: 'book', loadComponent: () => import('./pages/book/book.component').then(m => m.BookComponent) },
  { path: 'status/:token', loadComponent: () => import('./pages/status/status.component').then(m => m.StatusComponent) },
  { path: 'admin/login', loadComponent: () => import('./admin/login/login.component').then(m => m.LoginComponent) },
  {
    path: 'admin',
    loadComponent: () => import('./admin/layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./admin/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'appointments', loadComponent: () => import('./admin/appointments/appointments.component').then(m => m.AppointmentsComponent) },
      { path: 'barbers', loadComponent: () => import('./admin/barbers/barbers.component').then(m => m.BarbersComponent) },
      { path: 'services', loadComponent: () => import('./admin/services/services.component').then(m => m.ServicesComponent) },
      { path: 'clients', loadComponent: () => import('./admin/clients/clients.component').then(m => m.ClientsComponent) },
    ]
  },
  { path: '**', redirectTo: '' }
];
