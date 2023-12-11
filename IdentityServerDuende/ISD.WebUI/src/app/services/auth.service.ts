import { Injectable } from '@angular/core';
import { UserManager, User, UserManagerSettings } from 'oidc-client-ts';
import { Subject } from 'rxjs';

export class IdentityServerConfigs {
  public static URL_IDENTITY_SERVER: string = 'https://localhost:5001';
  public static CLIENT_ID: string = 'WebClient_ID';
  public static SCOPE_VARIABLES: string = 'openid profile main_api';
  public static RESPONSE_TYPE: string = 'code';
}

export const URL_INVEST_TRACKER: string = 'https://localhost:7201';
export const URL_CLIENT: string = 'http://localhost:4200';

export const PATH = {
  CLIENT: {
    HOME: '/',
    ACCOUNT: {
      SIGN_IN_CALLBACK: 'sign-in-callback',
      SIGN_OUT_CALLBACK: 'sign-out-callback',
    },
  },
  INVEST_TRACKER: {
    INVEST: {
      CORE: 'investments',
      CREATE: 'create',
    },
  },
};

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly _userManager: UserManager;
  public _user!: User | null;
  private _loginChangedSubject = new Subject<boolean>();
  public loginChanged = this._loginChangedSubject.asObservable();

  constructor() {
    this._userManager = new UserManager(this.idpSettings);
  }

  public async loginStart(): Promise<void> {
    await this._userManager.signinRedirect();
  }

  // Called after loginStart is successfully
  public loginFinish = (): Promise<User> => {
    return this._userManager.signinRedirectCallback().then((user: any) => {
      this._user = user;
      console.log('user -> ', this._user);
      this._loginChangedSubject.next(this.isUserExpired(user));
      return user;
    });
  };

  public async logoutStart() {
    await this._userManager.signoutRedirect();
  }

  // Called after logoutStart is successfully
  public logoutFinish() {
    this._user = null;
    return this._userManager.signoutRedirectCallback();
  }

  public isAuthenticated = (): Promise<boolean> => {
    return this._userManager.getUser().then((user: any) => {
      if (this._user !== user) {
        this._loginChangedSubject.next(this.isUserExpired(user));
      }

      this._user = user;
      return this.isUserExpired(user);
    });
  };

  public getAccessToken = (): Promise<string | null> => {
    return this._userManager.getUser().then((user: any) => {
      return !!user && !user.expired ? user.access_token : null;
    });
  };

  private isUserExpired = (user: User | null): boolean => {
    return !!user && !user.expired;
  };

  private get idpSettings(): UserManagerSettings {
    return {
      authority: IdentityServerConfigs.URL_IDENTITY_SERVER, // Identity Server
      client_id: IdentityServerConfigs.CLIENT_ID, // Main Client
      redirect_uri: `${URL_CLIENT}/${PATH.CLIENT.ACCOUNT.SIGN_IN_CALLBACK}`, // After authentication where we are redirected
      scope: IdentityServerConfigs.SCOPE_VARIABLES,
      response_type: IdentityServerConfigs.RESPONSE_TYPE,
      post_logout_redirect_uri: `${URL_CLIENT}/${PATH.CLIENT.ACCOUNT.SIGN_OUT_CALLBACK}`, // After logout where we are redirected
    };
  }
}

export class AuthConstants {
  public static clientRoot = 'http://localhost:4200';
  public static idpAuthority = 'https://localhost:5001/';
  public static clientId = 'WebClient_ID';
}
