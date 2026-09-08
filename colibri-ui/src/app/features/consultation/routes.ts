import { Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./consultation'),
  },
  {
    path: 'detail',
    loadComponent: () => import('./detail/detail'),
  },
  {
    path: 'clinical-note',
    loadComponent: () => import('./clinic-note/clinic-note'),
  },
];

export default routes;
