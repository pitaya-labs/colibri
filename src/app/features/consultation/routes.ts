import { Routes } from '@angular/router';
import Consultation from './consultation';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => Consultation,
  },
];

export default routes;
