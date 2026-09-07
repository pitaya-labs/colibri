import { Routes } from '@angular/router';
import Consultation from './consultation';

const routes: Routes = [
  {
    path: '',
    redirectTo: '',
    pathMatch: 'full',
  },
  {
    path: '',
    loadComponent: () => Consultation,
  },
];

export default routes;
