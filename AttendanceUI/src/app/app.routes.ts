import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [

  { 
    path: '', 
    pathMatch: 'full', 
    redirectTo: 'punch' 
  },

  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login').then((m) => m.Login)
  },

  {
    path: 'admin/register',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./features/register/register').then((m) => m.Register)
  },

  {
    path: 'punch',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/punch/punch').then((m) => m.Punch)
  },

  {
    path: 'admin/employees',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./features/admin/employee-list/employee-list')
        .then((m) => m.EmployeeList)
  },

  {
    path: 'admin/employees/new',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./features/admin/employee-form/employee-form')
        .then((m) => m.EmployeeForm)
  },

  {
    path: 'admin/employees/:id/edit',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./features/admin/employee-form/employee-form')
        .then((m) => m.EmployeeForm)
  },

  {
    path: 'admin/attendance',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./features/admin/attendance-list/attendance-list')
        .then((m) => m.AttendanceList)
  },

  {
    path: '**',
    redirectTo: 'punch'
  }
];