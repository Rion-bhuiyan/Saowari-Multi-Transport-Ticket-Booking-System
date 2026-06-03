import { Component, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { TokenService } from './core/services/token.service';
import * as signalR from '@microsoft/signalr';
import { environment } from '../environments/environment';
import { SettingsService } from './core/services/api/settings.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'saowari-client';
  enableBackgroundPattern = false;
  private hubConnection: signalR.HubConnection | null = null;

  constructor(
    private authService: AuthService, 
    private tokenService: TokenService,
    private settingsService: SettingsService,
    private renderer: Renderer2
  ) {}

  ngOnInit() {
    this.loadAppearanceSettings();
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

  private loadAppearanceSettings() {
    this.settingsService.getPublicSystemSettings().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          const patternStr = res.data.EnableBackgroundPattern || res.data.enableBackgroundPattern;
          this.enableBackgroundPattern = (patternStr && patternStr.toLowerCase() === 'true');
        }
      }
    });
  }

  private startPresenceTracking() {
    const hubUrl = environment.apiUrl.replace('/api', '/presenceHub');
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => this.tokenService.getAccessToken() || '' })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(err => console.error('Presence tracking failed', err));
  }
}
