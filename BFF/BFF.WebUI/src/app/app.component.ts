import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HomeComponent } from "./components/home.component";
import { AuthService } from './services/auth.service';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [CommonModule, RouterOutlet, HomeComponent, HttpClientModule],
  providers: [AuthService],
})
export class AppComponent {
  title = 'BFF.WebUI';

  public userAuthenticated: boolean = false;

  constructor(
    private authService: AuthService,
    private cdRef: ChangeDetectorRef
  ) {
    console.log('is user authenticated -> ', this.userAuthenticated);
    
    this.authService.isUserAuthenticated$.subscribe((userAuthenticated) => {
      console.log(
        'login changes, -> is user authenticaterd -> ',
        userAuthenticated
      );
      this.userAuthenticated = userAuthenticated;
      this.cdRef.detectChanges();
    });
  }

  ngOnInit() {
    this.authService.isAuthenticated().subscribe((user) => {
      this.userAuthenticated = user;
    });
  }

  onLogin() {
    this.authService.loginStart();
  }
}
