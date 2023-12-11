import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, PATH } from './services/auth.service';

@Component({
  selector: 'app-sign-out-redirect-callback',
  template: `<div></div>`,
  standalone: true,
})
export class SignOutRedirectCallbackComponent implements OnInit {
  constructor(private authService: AuthService, private router: Router) {}
  ngOnInit(): void {
    this.authService.logoutFinish().then((_) => {
      this.router.navigate([PATH.CLIENT.HOME], { replaceUrl: true });
    });
  }
}
