import { Routes } from '@angular/router';

const routes : Routes = [
  {
    path: '',
    loadComponent: () => import('./list/list').then(m => m.List),
  }
];

export default routes;
