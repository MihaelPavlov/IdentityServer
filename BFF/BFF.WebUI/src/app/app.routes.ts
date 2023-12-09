import { Routes } from '@angular/router';
import { HomeComponent } from './components/home.component';

export const routes: Routes = [
  { path: 'sign-in-callback', component: HomeComponent },
  { path: 'sign-out-callback', component: HomeComponent },
];
