import { Component, ElementRef, OnInit, ViewChild, AfterViewChecked, OnDestroy } from '@angular/core';
import { ChatService, SupportRoom, SupportMessage } from '../../../core/services/api/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/api/user.service';
import { Subscription } from 'rxjs';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-messenger',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-messenger.component.html',
  styleUrls: ['./admin-messenger.component.css']
})
export class AdminMessengerComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('messageContainer') private messageContainer!: ElementRef;
  @ViewChild('fileInput') private fileInput!: ElementRef;

  rooms: SupportRoom[] = [];
  filteredRooms: SupportRoom[] = [];
  selectedRoom: SupportRoom | null = null;
  messages: SupportMessage[] = [];
  newMessage = '';
  searchQuery = '';

  currentAdminId?: number;
  currentAdminName = 'Admin';

  // Selected room user profile
  selectedRoomUser: any = null;

  // Active User Info Modal Drawer
  showUserInfoModal = false;
  activeUserInfo: any = null;
  loadingUserInfo = false;

  // Expanded Lightbox Modal
  expandedPictureUrl: string | null = null;

  // Emoji Drawer
  showEmojiDrawer = false;
  popularEmojis = ['😀', '😂', '😍', '👍', '🙏', '🔥', '🎉', '💡', '🚌', '🎫', '❌', '⚠️', '❤️', '🙌', '✨', '👏'];

  // Image Canvas Resizer
  pendingImage: string | null = null;
  pendingImageFile: File | null = null;
  resizeScale = 0.8;
  originalSize = 0;
  resizedSize = 0;
  resizedBlob: Blob | null = null;

  // Theme Controller Mode
  isDarkMode = true;

  private subs: Subscription[] = [];

  Math = Math;

  constructor(
    private chatService: ChatService,
    private authService: AuthService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    // Resolve theme setting
    const savedTheme = localStorage.getItem('admin_theme');
    this.isDarkMode = savedTheme ? savedTheme === 'dark' : false;

    // Listen to global layout theme changes
    window.addEventListener('admin-theme-changed', this.onThemeChanged);

    // Resolve logged in admin profile details
    const currentUser = this.authService.currentUserValue;
    if (currentUser) {
      this.currentAdminId = currentUser.userId || (currentUser as any).userID;
      this.currentAdminName = currentUser.fullName;
    }

    // Connect to SignalR
    this.chatService.startConnection();

    this.subs.push(
      this.chatService.getConnectionStatus().subscribe(connected => {
        if (connected) {
          // Register this connection in the central Admins Broadcast Group
          this.chatService.registerAdminLobby();
          this.loadRooms();
        }
      })
    );

    // Dynamic Lobby updates
    this.subs.push(
      this.chatService.roomUpdate$.subscribe(room => {
        const idx = this.rooms.findIndex(r => r.id === room.id);
        if (idx !== -1) {
          this.rooms[idx] = { ...this.rooms[idx], ...room };
        } else {
          this.rooms.unshift(room);
        }
        this.applyFilter();
      })
    );

    this.subs.push(
      this.chatService.lobbyMessage$.subscribe(data => {
        const room = this.rooms.find(r => r.id === data.roomId);
        if (room) {
          room.lastMessageContent = data.content;
          room.lastMessageAt = data.lastMessageAt;
          if (this.selectedRoom?.id !== room.id) {
            room.unreadCount++;
          }
          // Resort
          this.rooms.sort((a, b) => new Date(b.lastMessageAt).getTime() - new Date(a.lastMessageAt).getTime());
          this.applyFilter();
        }
      })
    );

    // Claim triggers updates
    this.subs.push(
      this.chatService.roomAssigned$.subscribe(data => {
        const room = this.rooms.find(r => r.id === data.roomId);
        if (room) {
          room.assignedAdminId = data.adminId;
          room.assignedAdminName = data.adminName;
        }
        if (this.selectedRoom && this.selectedRoom.id === data.roomId) {
          this.selectedRoom.assignedAdminId = data.adminId;
          this.selectedRoom.assignedAdminName = data.adminName;
        }
      })
    );

    this.subs.push(
      this.chatService.roomReleased$.subscribe(data => {
        const room = this.rooms.find(r => r.id === data.roomId);
        if (room) {
          room.assignedAdminId = undefined;
          room.assignedAdminName = undefined;
        }
        if (this.selectedRoom && this.selectedRoom.id === data.roomId) {
          this.selectedRoom.assignedAdminId = undefined;
          this.selectedRoom.assignedAdminName = undefined;
        }
      })
    );

    this.subs.push(
      this.chatService.receiveMessage$.subscribe(msg => {
        if (this.selectedRoom && msg.roomId === this.selectedRoom.id) {
          this.messages.push(msg);
          this.scrollToBottom();
        }
      })
    );
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    window.removeEventListener('admin-theme-changed', this.onThemeChanged);
  }

  loadRooms(): void {
    this.chatService.getSupportRooms().subscribe({
      next: (res) => {
        this.rooms = res;
        this.applyFilter();
      }
    });
  }

  applyFilter(): void {
    if (!this.searchQuery) {
      this.filteredRooms = [...this.rooms];
    } else {
      const query = this.searchQuery.toLowerCase();
      this.filteredRooms = this.rooms.filter(r => 
        r.userEmailOrIP.toLowerCase().includes(query) || 
        (r.lastMessageContent && r.lastMessageContent.toLowerCase().includes(query))
      );
    }
  }

  selectRoom(room: SupportRoom): void {
    this.selectedRoom = room;
    room.unreadCount = 0;
    this.loadHistory(room.id);
    this.selectedRoomUser = null;

    if (room.userEmailOrIP.includes('@')) {
      this.userService.getByEmail(room.userEmailOrIP).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.selectedRoomUser = res.data;
          }
        }
      });
    }

    // Join room channel to observe
    if (this.currentAdminId) {
      this.chatService.adminJoinRoom(room.id, this.currentAdminId, this.currentAdminName);
    }
  }

  loadHistory(roomId: number): void {
    this.chatService.getRoomMessages(roomId).subscribe({
      next: (res) => {
        this.messages = res;
        this.scrollToBottom();
      }
    });
  }

  claimRoom(): void {
    if (!this.selectedRoom) return;
    this.chatService.claimRoom(this.selectedRoom.id).subscribe({
      next: () => {
        if (this.currentAdminId) {
          this.chatService.adminJoinRoom(this.selectedRoom!.id, this.currentAdminId, this.currentAdminName);
        }
      }
    });
  }

  releaseRoom(): void {
    if (!this.selectedRoom) return;
    this.chatService.releaseRoom(this.selectedRoom.id).subscribe({
      next: () => {
        this.chatService.adminLeaveRoom(this.selectedRoom!.id);
      }
    });
  }

  deleteMessage(messageId?: number): void {
    if (!messageId) return;
    if (confirm('Are you sure you want to delete this message? This action is permanent.')) {
      this.chatService.deleteSupportMessage(messageId).subscribe({
        next: (res) => {
          this.messages = this.messages.filter(m => m.id !== messageId);
          if (this.selectedRoom) {
            this.loadRooms();
          }
        },
        error: (err) => {
          alert('Failed to delete support message: ' + (err.error || err.message));
        }
      });
    }
  }

  sendMessage(): void {
    if (!this.newMessage.trim() || !this.selectedRoom || !this.selectedRoom.assignedAdminId) return;

    this.chatService.sendMessageToRoom(
      this.selectedRoom.id,
      'Support Agent',
      this.currentAdminId || null,
      this.newMessage,
      'text'
    );
    this.newMessage = '';
    this.showEmojiDrawer = false;
  }

  isCurrentUser(senderId?: number): boolean {
    return senderId == this.currentAdminId;
  }

  getFullUrl(relativeUrl?: string): string {
    return relativeUrl ? `http://localhost:5293${relativeUrl}` : '';
  }

  scrollToBottom(): void {
    try {
      if (this.messageContainer) {
        this.messageContainer.nativeElement.scrollTop = this.messageContainer.nativeElement.scrollHeight;
      }
    } catch (err) {}
  }

  // ── USER DETAILS DIALOG DRAWER ──────────────────────────────────────────────

  openUserInfo(): void {
    if (!this.selectedRoom) return;
    const emailOrIp = this.selectedRoom.userEmailOrIP;
    this.showUserInfoModal = true;
    this.activeUserInfo = null;

    if (emailOrIp.includes('@')) {
      this.loadingUserInfo = true;
      this.userService.getByEmail(emailOrIp).subscribe({
        next: (res) => {
          this.loadingUserInfo = false;
          if (res.success && res.data) {
            this.activeUserInfo = res.data;
          } else {
            this.activeUserInfo = { email: emailOrIp, isGuest: false, notFound: true };
          }
        },
        error: () => {
          this.loadingUserInfo = false;
          this.activeUserInfo = { email: emailOrIp, isGuest: false, notFound: true };
        }
      });
    } else {
      this.activeUserInfo = {
        email: emailOrIp,
        fullName: 'Guest Visitor',
        isGuest: true,
        phone: 'Not Available',
        roleName: 'Guest User',
        companyName: 'N/A',
        isActive: true,
        createdAt: this.selectedRoom.createdAt
      };
    }
  }

  closeUserInfo(): void {
    this.showUserInfoModal = false;
    this.activeUserInfo = null;
  }

  // ── PROFILE PICTURE ABSOLUTE RESOLVING & EXPANSION LIGHTBOX ─────────────────

  getProfilePictureUrl(path: string | null | undefined): string {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('data:')) {
      return path;
    }
    const cleanPath = path.startsWith('/') ? path : '/' + path;
    return 'http://localhost:5293' + cleanPath;
  }

  expandPicture(url: string | null | undefined): void {
    if (url) {
      this.expandedPictureUrl = this.getProfilePictureUrl(url);
    }
  }

  closeExpandedPicture(): void {
    this.expandedPictureUrl = null;
  }

  // ── EMOJI DRAWER ────────────────────────────────────────────────────────────

  toggleEmojiDrawer(): void {
    this.showEmojiDrawer = !this.showEmojiDrawer;
  }

  insertEmoji(emoji: string): void {
    this.newMessage += emoji;
  }

  // ── IMAGE CANVAS COMPRESSION ────────────────────────────────────────────────

  onFileSelected(event: any): void {
    const file = event.target.files[0] as File;
    if (!file) return;

    const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

    if (['.jpg', '.jpeg', '.png', '.webp', '.gif'].includes(extension)) {
      this.pendingImageFile = file;
      this.originalSize = file.size;

      // 5 MB max
      if (file.size > 5 * 1024 * 1024) {
        alert('Image exceeds the maximum upload limit of 5 MB.');
        this.cancelImagePending();
        return;
      }

      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.pendingImage = e.target.result;
        this.processImageCanvas();
      };
      reader.readAsDataURL(file);
    } 
    // Video verification (Min 20 MB)
    else if (['.mp4', '.mov', '.avi', '.mkv', '.webm'].includes(extension)) {
      if (file.size < 20 * 1024 * 1024) {
        alert('Validation Error: Video files must be at least 20 MB to be sent.');
        return;
      }
      this.uploadDirectAttachment(file, 'video');
    }
    // PDF / Docs
    else if (['.pdf', '.doc', '.docx'].includes(extension)) {
      const type = extension === '.pdf' ? 'pdf' : 'word';
      this.uploadDirectAttachment(file, type);
    }
    else {
      alert('Unsupported file format.');
    }
  }

  processImageCanvas(): void {
    if (!this.pendingImage) return;

    const img = new Image();
    img.src = this.pendingImage;
    img.onload = () => {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      const width = img.width * this.resizeScale;
      const height = img.height * this.resizeScale;
      canvas.width = width;
      canvas.height = height;

      ctx.drawImage(img, 0, 0, width, height);

      canvas.toBlob((blob) => {
        if (blob) {
          this.resizedBlob = blob;
          this.resizedSize = blob.size;
        }
      }, 'image/jpeg', 0.85);
    };
  }

  onResizeScaleChange(): void {
    this.processImageCanvas();
  }

  getKBSize(bytes: number): string {
    if (bytes === 0) return '0 KB';
    const kb = bytes / 1024;
    return kb > 1024 ? `${(kb / 1024).toFixed(1)} MB` : `${kb.toFixed(0)} KB`;
  }

  cancelImagePending(): void {
    this.pendingImage = null;
    this.pendingImageFile = null;
    this.resizedBlob = null;
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  uploadResizedImage(): void {
    if (!this.resizedBlob || !this.pendingImageFile || !this.selectedRoom) return;

    const file = new File([this.resizedBlob], this.pendingImageFile.name, {
      type: 'image/jpeg'
    });

    this.chatService.uploadChatFile(file, 'image').subscribe({
      next: (res) => {
        this.chatService.sendMessageToRoom(
          this.selectedRoom!.id,
          'Support Agent',
          this.currentAdminId || null,
          `Sent Image (${this.getKBSize(this.resizedSize)})`,
          'image',
          res.fileUrl
        );
        this.cancelImagePending();
      }
    });
  }

  uploadDirectAttachment(file: File, type: string): void {
    if (!this.selectedRoom) return;
    this.chatService.uploadChatFile(file, type).subscribe({
      next: (res) => {
        this.chatService.sendMessageToRoom(
          this.selectedRoom!.id,
          'Support Agent',
          this.currentAdminId || null,
          file.name,
          type,
          res.fileUrl
        );
      }
    });
  }

  // ── THEME SWITCHING DYNAMIC METHODS ───────────────────────────────────────

  onThemeChanged = (event: any): void => {
    if (event.detail && typeof event.detail.isDarkMode === 'boolean') {
      this.isDarkMode = event.detail.isDarkMode;
    }
  };

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    localStorage.setItem('admin_theme', this.isDarkMode ? 'dark' : 'light');
    // Dispatch global layout event
    window.dispatchEvent(new CustomEvent('admin-theme-changed', { detail: { isDarkMode: this.isDarkMode } }));
  }
}
