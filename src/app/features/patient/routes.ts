import { Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    redirectTo: '',
    pathMatch: 'full',
  },
  {
    path: '',
    loadComponent: () => import('./list/list').then((m) => m.List),
  },
  {
    path: 'create',
    loadComponent: () => import('./create-edit/create-edit').then((m) => m.CreateEdit),
  },
];

export default routes;
