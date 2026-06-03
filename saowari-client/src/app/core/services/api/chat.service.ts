import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { TokenService } from '../token.service';

export interface SupportRoom {
  id: number;
  userEmailOrIP: string;
  assignedAdminId?: number;
  assignedAdminName?: string;
  isActive: boolean;
  createdAt: string;
  lastMessageAt: string;
  unreadCount: number;
  lastMessageContent?: string;
}

export interface SupportMessage {
  id?: number;
  roomId: number;
  senderName: string;
  senderId?: number;
  content: string;
  messageType: string;
  fileUrl?: string;
  createdAt?: string;
  isRead?: boolean;
}

export interface ScheduleChatMessage {
  id?: number;
  scheduleId: number;
  senderId: number;
  senderName: string;
  content: string;
  messageType: string;
  fileUrl?: string;
  createdAt?: string;
}

export interface ScheduleChatMember {
  userId: number;
  fullName: string;
  role: string;
  isRemoved: boolean;
  removedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private hubConnection!: signalR.HubConnection;
  private baseUrl = 'http://localhost:5293';
  
  // Real-time Event Streams
  public roomJoined$ = new Subject<number>();
  public receiveMessage$ = new Subject<SupportMessage>();
  public roomUpdate$ = new Subject<SupportRoom>();
  public lobbyMessage$ = new Subject<any>();
  public roomAssigned$ = new Subject<any>();
  public roomReleased$ = new Subject<any>();
  public adminPresence$ = new Subject<any>();
  public receiveScheduleMessage$ = new Subject<ScheduleChatMessage>();
  public systemMessage$ = new Subject<string>();
  public userRemovedFromGroup$ = new Subject<any>();

  private connectionEstablished$ = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient, private tokenService: TokenService) {}

  // ── SIGNALR HUB CONNECTION MANAGEMENT ───────────────────────────────────────

  public startConnection(): void {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const hubUrl = `${environment.apiUrl.replace('/api', '')}/chatHub`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.tokenService.getAccessToken() || ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR ChatHub connection established successfully!');
        this.connectionEstablished$.next(true);
        this.registerHandlers();
      })
      .catch(err => {
        console.error('Error starting SignalR connection:', err);
        setTimeout(() => this.startConnection(), 5000);
      });
  }

  public getConnectionStatus(): Observable<boolean> {
    return this.connectionEstablished$.asObservable();
  }

  private registerHandlers(): void {
    this.hubConnection.on('RoomJoined', (roomId: number) => this.roomJoined$.next(roomId));
    this.hubConnection.on('ReceiveMessage', (msg: SupportMessage) => this.receiveMessage$.next(msg));
    this.hubConnection.on('ReceiveRoomUpdate', (room: SupportRoom) => this.roomUpdate$.next(room));
    this.hubConnection.on('ReceiveLobbyMessage', (data: any) => this.lobbyMessage$.next(data));
    this.hubConnection.on('RoomAssigned', (data: any) => this.roomAssigned$.next(data));
    this.hubConnection.on('RoomReleased', (data: any) => this.roomReleased$.next(data));
    this.hubConnection.on('AdminPresence', (data: any) => this.adminPresence$.next(data));
    this.hubConnection.on('ReceiveScheduleMessage', (msg: ScheduleChatMessage) => this.receiveScheduleMessage$.next(msg));
    this.hubConnection.on('ReceiveSystemMessage', (text: string) => this.systemMessage$.next(text));
    this.hubConnection.on('UserRemovedFromGroup', (data: any) => this.userRemovedFromGroup$.next(data));
  }

  // ── HUB CALL TRIGGERS ───────────────────────────────────────────────────────

  public joinSupportRoom(userEmailOrIP: string): Promise<void> {
    return this.hubConnection.invoke('JoinSupportRoom', userEmailOrIP);
  }

  public adminJoinRoom(roomId: number, adminId: number, adminName: string): Promise<void> {
    return this.hubConnection.invoke('AdminJoinRoom', roomId, adminId, adminName);
  }

  public adminLeaveRoom(roomId: number): Promise<void> {
    return this.hubConnection.invoke('AdminLeaveRoom', roomId);
  }

  public sendMessageToRoom(roomId: number, senderName: string, senderId: number | null, content: string, type: string, fileUrl?: string): Promise<void> {
    return this.hubConnection.invoke('SendMessageToRoom', roomId, senderName, senderId, content, type, fileUrl || null);
  }

  public registerAdminLobby(): Promise<void> {
    return this.hubConnection.invoke('RegisterAdminLobby');
  }

  public joinScheduleGroup(scheduleId: number, userId: number, fullName: string): Promise<void> {
    return this.hubConnection.invoke('JoinScheduleGroup', scheduleId, userId, fullName);
  }

  public sendMessageToSchedule(scheduleId: number, senderId: number, senderName: string, content: string, type: string, fileUrl?: string): Promise<void> {
    return this.hubConnection.invoke('SendMessageToSchedule', scheduleId, senderId, senderName, content, type, fileUrl || null);
  }

  // ── REST HTTP CLIENT SERVICES ──────────────────────────────────────────────

  public getSupportRooms(): Observable<SupportRoom[]> {
    return this.http.get<SupportRoom[]>(`${this.baseUrl}/api/chat/rooms`);
  }

  public getRoomMessages(roomId: number): Observable<SupportMessage[]> {
    return this.http.get<SupportMessage[]>(`${this.baseUrl}/api/chat/rooms/${roomId}/messages`);
  }

  public claimRoom(roomId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/api/chat/rooms/${roomId}/claim`, {});
  }

  public releaseRoom(roomId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/api/chat/rooms/${roomId}/release`, {});
  }

  public deleteSupportMessage(messageId: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/api/chat/messages/${messageId}`);
  }

  public getPassengerActiveSchedules(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/api/chat/passenger/active-schedules`);
  }

  public getScheduleMessages(scheduleId: number): Observable<ScheduleChatMessage[]> {
    return this.http.get<ScheduleChatMessage[]>(`${this.baseUrl}/api/chat/schedule/${scheduleId}/messages`);
  }

  public getScheduleMembers(scheduleId: number): Observable<ScheduleChatMember[]> {
    return this.http.get<ScheduleChatMember[]>(`${this.baseUrl}/api/chat/schedule/${scheduleId}/members`);
  }

  public removeUserFromSchedule(scheduleId: number, memberId: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/api/chat/schedule/${scheduleId}/members/${memberId}`);
  }

  public uploadChatFile(file: File, type: string): Observable<{ fileUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('fileType', type);
    return this.http.post<{ fileUrl: string }>(`${this.baseUrl}/api/chatuploads/upload`, formData);
  }

  // Helper: Retrieve Client IP safely or fallback
  public getClientIP(): Observable<{ ip: string }> {
    // Falls back to a reliable public IP lookup API to identify guests
    return this.http.get<{ ip: string }>('https://api.ipify.org?format=json');
  }
}
