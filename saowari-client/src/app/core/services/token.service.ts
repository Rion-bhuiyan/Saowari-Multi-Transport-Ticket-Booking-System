import { Injectable } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { environment } from '../../../environments/environment';
import { UserModel } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class TokenService {

  constructor(private cookieService: CookieService) { }

  setAccessToken(token: string, expiryMinutes: number): void {
    const expireDate = new Date();
    expireDate.setMinutes(expireDate.getMinutes() + expiryMinutes);
    this.cookieService.set(environment.accessTokenKey, token, expireDate, '/', environment.cookieDomain, environment.production, 'Strict');
    this.cookieService.set(environment.tokenExpiryKey, expireDate.getTime().toString(), expireDate, '/', environment.cookieDomain, environment.production, 'Strict');
  }

  setRefreshToken(token: string): void {
    const expireDate = new Date();
    expireDate.setDate(expireDate.getDate() + 7); // 7 days
    this.cookieService.set(environment.refreshTokenKey, token, expireDate, '/', environment.cookieDomain, environment.production, 'Strict');
  }

  getAccessToken(): string | null {
    return this.cookieService.check(environment.accessTokenKey) ? this.cookieService.get(environment.accessTokenKey) : null;
  }

  getRefreshToken(): string | null {
    return this.cookieService.check(environment.refreshTokenKey) ? this.cookieService.get(environment.refreshTokenKey) : null;
  }

  setUser(user: UserModel): void {
    this.cookieService.set(environment.userKey, JSON.stringify(user), 7, '/', environment.cookieDomain, environment.production, 'Strict');
  }

  getUser(): UserModel | null {
    if (this.cookieService.check(environment.userKey)) {
      try {
        return JSON.parse(this.cookieService.get(environment.userKey)) as UserModel;
      } catch {
        return null;
      }
    }
    return null;
  }

  clearAll(): void {
    this.cookieService.delete(environment.accessTokenKey, '/', environment.cookieDomain);
    this.cookieService.delete(environment.refreshTokenKey, '/', environment.cookieDomain);
    this.cookieService.delete(environment.userKey, '/', environment.cookieDomain);
    this.cookieService.delete(environment.tokenExpiryKey, '/', environment.cookieDomain);
  }

  isTokenExpired(): boolean {
    if (!this.cookieService.check(environment.tokenExpiryKey)) return true;
    const expiryTime = parseInt(this.cookieService.get(environment.tokenExpiryKey), 10);
    // Add 1 minute buffer
    return new Date().getTime() > (expiryTime - 60000);
  }
}
