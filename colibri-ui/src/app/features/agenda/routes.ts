import { Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./list/list'),
  },
  {
    path: 'create',
    loadComponent: () => import('./create/create').then((m) => m.Create),
  },
];

export default routes;
