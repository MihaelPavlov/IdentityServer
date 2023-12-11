import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';
import { User } from 'oidc-client-ts';

@Component({
  selector: 'home',
  template: `
    <div>
      <button (click)="getCurrentUserInfo()">Current User Info</button>

      <br />

      <p>Current User Info</p>
      <p>Id: {{ userInfo?.profile?.sub }}</p>
      <p>Access Token: {{ userInfo?.access_token }}</p>
    </div>

    <button (click)="onLogout()">Logout</button>
  `,
  standalone: true,
})
export class HomeComponent {
  public userInfo: User | null = null;

  constructor(private authService: AuthService) {}

  getCurrentUserInfo() {
    this.userInfo = this.authService._user;
    console.log('user-info ->' , this.userInfo)
  }

  onLogout() {
    this.authService.logoutStart();
  }
}
