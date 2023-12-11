import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, PATH } from './services/auth.service';

@Component({
  selector: 'app-sign-in-redirect-callback',
  template: `<div></div>`,
  standalone: true,
})
export class SignInRedirectCallbackComponent implements OnInit {
  constructor(private _authService: AuthService, private _router: Router) {}
  ngOnInit(): void {
    this._authService.loginFinish().then((_) => {
      this._router.navigate([PATH.CLIENT.HOME], { replaceUrl: true });
    });
  }
}
