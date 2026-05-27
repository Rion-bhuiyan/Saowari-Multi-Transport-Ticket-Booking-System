import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { TokenService } from './core/services/token.service';
import * as signalR from '@microsoft/signalr';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'saowari-client';
  private hubConnection: signalR.HubConnection | null = null;

  constructor(private authService: AuthService, private tokenService: TokenService) {}

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (user && !this.hubConnection) {
        this.startPresenceTracking();
      } else if (!user && this.hubConnection) {
        this.hubConnection.stop();
        this.hubConnection = null;
      }
    });
  }

  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private startPresenceTracking() {
    const hubUrl = environment.apiUrl.replace('/api', '/presenceHub');
    const token = this.tokenService.getAccessToken(); 

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token || '' })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(err => console.error('Presence tracking failed', err));
  }
}
