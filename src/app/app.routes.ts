import { Routes } from '@angular/router';
import consultationRoutes from './features/consultation/routes';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/main/main'),
    children: [
      {
        path: '',
        redirectTo: 'agenda',
        pathMatch: 'full',
      },
      {
        path: 'agenda',
        loadChildren: () => import('./features/agenda/routes'),
      },
      {
        path: 'patients',
        loadChildren: () => import('./features/patient/routes'),
      },
      {
        path: 'consultation',
        loadChildren: () => consultationRoutes,
      },
    ],
  },
];
