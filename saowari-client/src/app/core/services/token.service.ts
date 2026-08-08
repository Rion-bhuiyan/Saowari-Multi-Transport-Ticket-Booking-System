import { Injectable } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { environment } from '../../../environments/environment';
import { UserModel } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class TokenService {

  constructor(private cookieService: CookieService) { }

  private getDomain(): string | undefined {
    return (environment.cookieDomain && environment.cookieDomain.trim() !== '') ? environment.cookieDomain : undefined;
  }

  setAccessToken(token: string, expiryMinutes: number): void {
    const expireDate = new Date();
    expireDate.setMinutes(expireDate.getMinutes() + expiryMinutes);
    const domain = this.getDomain();
    try {
      this.cookieService.set(environment.accessTokenKey, token, expireDate, '/', domain, false, 'Lax');
      this.cookieService.set(environment.tokenExpiryKey, expireDate.getTime().toString(), expireDate, '/', domain, false, 'Lax');
    } catch (e) {}
    localStorage.setItem(environment.accessTokenKey, token);
    localStorage.setItem(environment.tokenExpiryKey, expireDate.getTime().toString());
  }

  setRefreshToken(token: string): void {
    const expireDate = new Date();
    expireDate.setDate(expireDate.getDate() + 7);
    const domain = this.getDomain();
    try {
      this.cookieService.set(environment.refreshTokenKey, token, expireDate, '/', domain, false, 'Lax');
    } catch (e) {}
    localStorage.setItem(environment.refreshTokenKey, token);
  }

  getAccessToken(): string | null {
    if (this.cookieService.check(environment.accessTokenKey)) {
      return this.cookieService.get(environment.accessTokenKey);
    }
    return localStorage.getItem(environment.accessTokenKey);
  }

  getRefreshToken(): string | null {
    if (this.cookieService.check(environment.refreshTokenKey)) {
      return this.cookieService.get(environment.refreshTokenKey);
    }
    return localStorage.getItem(environment.refreshTokenKey);
  }

  setUser(user: UserModel): void {
    const domain = this.getDomain();
    const json = JSON.stringify(user);
    try {
      this.cookieService.set(environment.userKey, json, 7, '/', domain, false, 'Lax');
    } catch (e) {}
    localStorage.setItem(environment.userKey, json);
  }

  getUser(): UserModel | null {
    let raw: string | null = null;
    if (this.cookieService.check(environment.userKey)) {
      raw = this.cookieService.get(environment.userKey);
    } else {
      raw = localStorage.getItem(environment.userKey);
    }
    if (raw) {
      try {
        return JSON.parse(raw) as UserModel;
      } catch {
        return null;
      }
    }
    return null;
  }

  clearAll(): void {
    const domain = this.getDomain();
    try {
      this.cookieService.delete(environment.accessTokenKey, '/', domain);
      this.cookieService.delete(environment.refreshTokenKey, '/', domain);
      this.cookieService.delete(environment.userKey, '/', domain);
      this.cookieService.delete(environment.tokenExpiryKey, '/', domain);
    } catch (e) {}
    localStorage.removeItem(environment.accessTokenKey);
    localStorage.removeItem(environment.refreshTokenKey);
    localStorage.removeItem(environment.userKey);
    localStorage.removeItem(environment.tokenExpiryKey);
  }

  isTokenExpired(): boolean {
    let expiryStr: string | null = null;
    if (this.cookieService.check(environment.tokenExpiryKey)) {
      expiryStr = this.cookieService.get(environment.tokenExpiryKey);
    } else {
      expiryStr = localStorage.getItem(environment.tokenExpiryKey);
    }
    if (!expiryStr) return true;
    const expiryTime = parseInt(expiryStr, 10);
    return new Date().getTime() > (expiryTime - 60000);
  }
}

