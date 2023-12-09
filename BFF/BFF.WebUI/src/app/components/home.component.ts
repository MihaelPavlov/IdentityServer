import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'home',
  template: `
    <div>
      <button (click)="getCurrentUserInfo()">Current User Info</button>

      <br />

      <p>Current User Info</p>
      <p>Id: {{ userInfo?.id }}</p>
      <p>Email: {{ userInfo?.email }}</p>
    </div>

    <button (click)="onLogout()">Logout</button>
  `,
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  providers: [AuthService],
})
export class HomeComponent {
  public userInfo: any | null = null;

  constructor(private authService: AuthService) {}

  getCurrentUserInfo() {
    this.authService.getUserInfo();
    this.authService.user$.subscribe((response) => {
      console.log('user response -> ', response);
      this.userInfo = response;
    });
  }
  
  onLogout() {
    this.authService.logoutStart();
  }
}
