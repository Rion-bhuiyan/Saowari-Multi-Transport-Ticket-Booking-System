import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginDto, RegisterDto, UserModel } from '../models/auth.model';
import { ApiResponse } from '../models/common.model';
import { TokenService } from './token.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  public currentUser$ = new BehaviorSubject<UserModel | null>(null);

  constructor(
    private http: HttpClient, 
    private tokenService: TokenService,
    private router: Router
  ) {
    this.currentUser$.next(this.tokenService.getUser());
  }

  get currentUserValue(): UserModel | null {
    return this.currentUser$.value;
  }

  updateCurrentUser(user: UserModel): void {
    this.tokenService.setUser(user);
    this.currentUser$.next(user);
  }

  login(dto: LoginDto): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, dto).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.handleLoginResponse(response.data);
        }
      })
    );
  }

  verifyLoginOtp(email: string, otpCode: string): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/verify-login-otp`, { email, otpCode }).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.handleLoginResponse(response.data);
        }
      })
    );
  }

  verifyRegistrationOtp(email: string, otpCode: string): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/verify-registration-otp`, { email, otpCode }).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.handleLoginResponse(response.data);
        }
      })
    );
  }

  handleLoginResponse(data: AuthResponse): void {
    this.tokenService.setAccessToken(data.accessToken, 300);
    this.tokenService.setRefreshToken(data.refreshToken);
    this.tokenService.setUser(data.user);
    this.currentUser$.next(data.user);
  }

  register(dto: RegisterDto): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, dto).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.tokenService.setAccessToken(response.data.accessToken, 300);
          this.tokenService.setRefreshToken(response.data.refreshToken);
          this.tokenService.setUser(response.data.user);
          this.currentUser$.next(response.data.user);
        }
      })
    );
  }

  logout(): void {
    this.tokenService.clearAll();
    this.currentUser$.next(null);
    this.router.navigate(['/auth/login']);
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const rToken = this.tokenService.getRefreshToken();
    if (!rToken) return throwError(() => new Error('No refresh token available'));

    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/refresh-token`, { refreshToken: rToken }).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.tokenService.setAccessToken(response.data.accessToken, 300);
          this.tokenService.setRefreshToken(response.data.refreshToken);
          this.tokenService.setUser(response.data.user);
          this.currentUser$.next(response.data.user);
        }
      })
    );
  }

  changePassword(dto: any): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/change-password`, dto);
  }

  forgotPassword(email: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: any): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/reset-password`, data);
  }

  isLoggedIn(): boolean {
    return !!this.tokenService.getAccessToken();
  }

  getCurrentUser(): UserModel | null {
    return this.currentUser$.value;
  }

  hasRole(roleName: string): boolean {
    const userRole = this.getRoleName();
    if (!userRole) return false;
    return userRole.trim().toLowerCase() === roleName.trim().toLowerCase();
  }

  getRoleName(): string {
    const user = this.getCurrentUser();
    if (user && user.roleName) return user.roleName;

    const token = this.tokenService.getAccessToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || payload.Role || '';
      } catch (e) {}
    }
    return '';
  }

  isAdmin(): boolean {
    return this.hasRole('Admin') || this.hasRole('System Administrator');
  }

  isAgent(): boolean {
    return this.hasRole('Agent');
  }

  isCompanyManager(): boolean {
    return this.hasRole('CompanyManager') || this.hasRole('Company Manager');
  }

  isSupervisor(): boolean {
    return this.hasRole('Supervisor');
  }

  isDriver(): boolean {
    return this.hasRole('Driver');
  }

  canAccessAdminPanel(): boolean {
    return this.isAdmin() || this.isAgent() || this.isCompanyManager() || this.isSupervisor() || this.isDriver();
  }
}
