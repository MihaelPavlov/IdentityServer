import { Routes } from '@angular/router';
import { SignInRedirectCallbackComponent } from './sign-in-redirect-callback.component';
import { SignOutRedirectCallbackComponent } from './sign-out-redirect-callback.component';
import { HomeComponent } from './home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'sign-in-callback', component: SignInRedirectCallbackComponent },
  { path: 'sign-out-callback', component: SignOutRedirectCallbackComponent },
];
