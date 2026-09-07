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
];

export default routes;
