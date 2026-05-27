import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { AuthService } from '../auth.service';

export interface NotificationItem {
  id: number;
  userId?: number;
  companyId?: number;
  companyName?: string;
  title: string;
  message: string;
  type: string;
  entityType?: string;
  entityId?: number;
  icon: string;
  colorClass: string;
  isRead: boolean;
  createdAt: string;
}

export interface AdminNotificationPreference {
  id: number;
  adminUserId: number;
  companyId: number;
  companyName: string;
  isEnabled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;
  
  private _unreadCount$ = new BehaviorSubject<number>(0);
  unreadCount$ = this._unreadCount$.asObservable();

  private _newNotification$ = new Subject<NotificationItem>();
  newNotification$ = this._newNotification$.asObservable();

  private hubConnection: signalR.HubConnection | null = null;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.startConnection();
  }

  public startConnection(): void {
    if (!this.authService.isLoggedIn()) {
      return;
    }

    const token = localStorage.getItem('token');
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api', '')}/notificationHub`, {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('Real-time Notification SignalR Connection started');
        this.addNotificationListener();
      })
      .catch(err => console.log('Error while starting notification connection: ' + err));
  }

  private addNotificationListener(): void {
    if (!this.hubConnection) return;
    
    this.hubConnection.on('ReceiveNotification', (notification: NotificationItem) => {
      // Update unread count immediately
      this._unreadCount$.next(this._unreadCount$.value + 1);
      // Emit the notification so navbar can show it
      this._newNotification$.next(notification);
    });
  }

  getAll(): Observable<any> {
    return this.http.get<any>(this.apiUrl).pipe(
      tap(res => {
        if (res.success && res.data) {
          const count = res.data.filter((n: NotificationItem) => !n.isRead).length;
          this._unreadCount$.next(count);
        }
      })
    );
  }

  getUnreadCount(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/unread-count`).pipe(
      tap(res => {
        if (res.success) this._unreadCount$.next(res.data);
      })
    );
  }

  markAsRead(id: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => {
        const current = this._unreadCount$.value;
        if (current > 0) this._unreadCount$.next(current - 1);
      })
    );
  }

  markAllAsRead(): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => this._unreadCount$.next(0))
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  clearAll(): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/clear-all`).pipe(
      tap(() => this._unreadCount$.next(0))
    );
  }

  getAdminPreferences(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/admin-preferences`);
  }

  togglePreference(companyId: number, isEnabled: boolean): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/admin-preferences/${companyId}`, { isEnabled });
  }
}
