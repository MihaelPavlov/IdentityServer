import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  title = 'ISD.WebUI';
  public userAuthenticated = false;
  constructor(
    private _authService: AuthService,
    private cdRef: ChangeDetectorRef
  ) {
    this._authService.loginChanged.subscribe((userAuthenticated) => {
      console.log(
        'login changes, -> is user authenticaterd -> ',
        userAuthenticated
      );
      this.userAuthenticated = userAuthenticated;
      this.cdRef.detectChanges();
    });
  }

  ngOnInit(): void {
    this._authService.isAuthenticated().then((userAuthenticated) => {
      this.userAuthenticated = userAuthenticated;
      console.log(' is user authenticaterd -> ', userAuthenticated);
      this.cdRef.detectChanges();
    });
  }

  onLogin(){
    this._authService.loginStart();
  }
}
