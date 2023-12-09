import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PersistenceService } from './persistence.service';
import { BehaviorSubject, Observable, Subject, map } from 'rxjs';
import * as CryptoJS from 'crypto-js';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly state_Key: string = 'state';
  private readonly codeVerifier_Key: string = 'codeVerifier';
  private isUserAuthenticatedSubject$ = new Subject<boolean>();
  private userInfo$ = new BehaviorSubject<any | null>(null);

  public isUserAuthenticated$ = this.isUserAuthenticatedSubject$.asObservable();
  public user$ = this.userInfo$.asObservable();

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private http: HttpClient,
    private persistenceService: PersistenceService
  ) {
    this.activatedRoute.queryParams.subscribe((params: any) => {
      if (params.code) {
        this.getCookieFromApi(params.code, params.state);
      }
    });
  }

  public loginStart(): void {
    const state = this.strRandom(40);
    const codeVerifier = this.strRandom(128);

    this.persistenceService.setLocalStorageItem(this.state_Key, state);
    this.persistenceService.setLocalStorageItem(
      this.codeVerifier_Key,
      codeVerifier
    );

    const codeVerifierHash = CryptoJS.SHA256(codeVerifier).toString(
      CryptoJS.enc.Base64
    );
    const codeChallenge = codeVerifierHash
      .replace(/=/g, '')
      .replace(/\+/g, '-')
      .replace(/\//g, '_');

    const params = [
      'client_id=' + 'WebClient_ID',
      'redirect_uri=' +
        encodeURIComponent(`${Settings(SettingType.Client)}/sign-in-callback`),
      'response_type=code',
      'scope=' + 'openid profile main_api IdentityServerApi',
      'state=' + state,
      'code_challenge=' + codeChallenge,
      'code_challenge_method=S256',
      'response_mode=query',
    ];
    const encoded = encodeURIComponent(
      '/connect/authorize/callback?' + params.join('&')
    );

    window.location.href =
      `${Settings(SettingType.IdentityServer)}/Account/Login` + '?ReturnUrl=' + encoded;
  }

  public logoutStart(): void {
    this.http
      .get(`${Settings(SettingType.BackEnd)}/account/logout`, {
        withCredentials: true,
      })
      .subscribe({
        next: (response) => {
          this.persistenceService.removeLocalStorageItem(this.state_Key);
          this.persistenceService.removeLocalStorageItem(this.codeVerifier_Key);
          this.userInfo$.next(null);
          window.location.href = Settings(SettingType.Client);
        },
        error: (error) => {
          console.warn('HTTP Error', error);
        },
      });
  }

  public getCookieFromApi(code: string, state: string): void {
    if (state !== this.persistenceService.getLocalStorageItem(this.state_Key)) {
      alert('Invalid callBack state');
      return;
    }

    const codeVerifier = this.persistenceService.getLocalStorageItem(
      this.codeVerifier_Key
    );
    if (!codeVerifier) {
      alert('codeVerifier in localStorage is expected');
      return;
    }

    this.http
      .get<any>(`${Settings(SettingType.BackEnd)}/account/authorize`, {
        withCredentials: true,
        headers: {
          'code': code,
          'code_verifier': codeVerifier,
        },
      })
      .subscribe({
        next: (response) => {
          if (response !== null) {
            this.userInfo$.next({ id: response.sub, email: response.name });

            this.isUserAuthenticatedSubject$.next(true);

            this.router.navigate(['/'], { replaceUrl: true });
          }

          console.log('cookie from api authrorize -> ', response);
        },
        error: (error) => {
          console.warn('HTTP Error', error);
        },
      });
  }

  public isAuthenticated = (): Observable<boolean> => {
    console.log('user ---->', this.userInfo$.value);
    return this.userInfo$.pipe(
      map((user) => {
        return !!user;
      })
    );
  };

  public getUserInfo(): void {
    this.http
      .get<any>(`${Settings(SettingType.BackEnd)}/account/user-info`, {
        withCredentials: true,
      })
      .subscribe({
        next: (response) => {
          this.userInfo$.next({
            id: response.subFromClaim,
            email: response.email,
          });
        },
        error: (error) => {
          console.warn('HTTP Error', error);
        },
      });
  }

  private strRandom(length: number): string {
    let result = '';
    const characters =
      'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;
    for (let i = 0; i < length; i++) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
  }
}

export const Settings = (setting: SettingType): string => {
  switch (setting) {
    case SettingType.Client:
      return 'http://localhost:4200';
    case SettingType.IdentityServer:
      return 'https://localhost:5001';
    case SettingType.BackEnd:
      return 'https://localhost:7201';
    default:
      return ''
  }
};

export enum SettingType {
  Client = 0,
  IdentityServer = 1,
  BackEnd = 2,
}
