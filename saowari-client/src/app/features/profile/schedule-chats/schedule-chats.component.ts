import { Component, ElementRef, OnInit, ViewChild, AfterViewChecked, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ChatService, ScheduleChatMessage } from '../../../core/services/api/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-schedule-chats',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './schedule-chats.component.html',
  styleUrls: ['./schedule-chats.component.css']
})
export class ScheduleChatsComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('messageContainer') private messageContainer!: ElementRef;
  @ViewChild('fileInput') private fileInput!: ElementRef;

  schedules: any[] = [];
  selectedSchedule: any = null;
  messages: ScheduleChatMessage[] = [];
  members: any[] = [];
  newMessage = '';

  currentUserId?: number;
  currentUserFullName = 'Passenger';
  canManageGroup = false;

  // Emoji Picker Drawer
  showEmojiDrawer = false;
  popularEmojis = ['😀', '😂', '😍', '👍', '🙏', '🔥', '🎉', '💡', '🚌', '🎫', '❌', '⚠️', '❤️', '🙌', '✨', '👏'];

  // Image Canvas Resizer
  pendingImage: string | null = null;
  pendingImageFile: File | null = null;
  resizeScale = 0.8;
  originalSize = 0;
  resizedSize = 0;
  resizedBlob: Blob | null = null;

  private subs: Subscription[] = [];

  Math = Math;

  constructor(
    private chatService: ChatService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Resolve logged in profile
    const user = this.authService.currentUserValue;
    if (user) {
      this.currentUserId = user.userId || (user as any).userID;
      this.currentUserFullName = user.fullName;
      this.canManageGroup = this.authService.isSupervisor() || this.authService.isDriver() || this.authService.isAdmin() || this.authService.isCompanyManager();
    }

    // Connect to SignalR
    this.chatService.startConnection();

    this.subs.push(
      this.chatService.getConnectionStatus().subscribe(connected => {
        if (connected) {
          this.loadSchedules();
        }
      })
    );

    // Group message listener
    this.subs.push(
      this.chatService.receiveScheduleMessage$.subscribe(msg => {
        if (this.selectedSchedule && msg.scheduleId === this.selectedSchedule.scheduleID) {
          this.messages.push(msg);
          this.scrollToBottom();
        }
      })
    );

    // Member removal listener
    this.subs.push(
      this.chatService.userRemovedFromGroup$.subscribe(data => {
        if (this.selectedSchedule && data.scheduleId === this.selectedSchedule.scheduleID) {
          const m = this.members.find(x => x.userId === data.userId);
          if (m) {
            m.isRemoved = true;
          }
        }
      })
    );

    // Optional: System alerts listener
    this.subs.push(
      this.chatService.systemMessage$.subscribe(text => {
        if (this.selectedSchedule) {
          this.messages.push({
            scheduleId: this.selectedSchedule.scheduleID,
            senderId: 0,
            senderName: 'SYSTEM ALERT',
            content: text,
            messageType: 'text',
            createdAt: new Date().toISOString()
          });
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
  }

  loadSchedules(): void {
    this.chatService.getPassengerActiveSchedules().subscribe({
      next: (res) => {
        this.schedules = res;
      }
    });
  }

  selectSchedule(item: any): void {
    this.selectedSchedule = item;
    this.loadHistory(item.scheduleID);

    // Join schedule group in SignalR
    if (this.currentUserId) {
      this.chatService.joinScheduleGroup(item.scheduleID, this.currentUserId, this.currentUserFullName);
    }
  }

  loadHistory(scheduleId: number): void {
    this.chatService.getScheduleMessages(scheduleId).subscribe({
      next: (res) => {
        this.messages = res;
        this.scrollToBottom();
      }
    });
    this.chatService.getScheduleMembers(scheduleId).subscribe({
      next: (res) => {
        this.members = res;
      }
    });
  }

  removeMember(memberId: number): void {
    if (!this.selectedSchedule || !this.canManageGroup) return;
    if (confirm('Are you sure you want to remove this user from the chat group?')) {
      this.chatService.removeUserFromSchedule(this.selectedSchedule.scheduleID, memberId).subscribe({
        next: (res) => {
          if (res.success) {
            const m = this.members.find(x => x.userId === memberId);
            if (m) m.isRemoved = true;
          }
        }
      });
    }
  }

  sendMessage(): void {
    if (!this.newMessage.trim() || !this.selectedSchedule || !this.currentUserId) return;

    this.chatService.sendMessageToSchedule(
      this.selectedSchedule.scheduleID,
      this.currentUserId,
      this.currentUserFullName,
      this.newMessage,
      'text'
    );
    this.newMessage = '';
    this.showEmojiDrawer = false;
  }

  isCurrentUser(senderId: number): boolean {
    return senderId === this.currentUserId;
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

  // ── EMOJI PICKER ────────────────────────────────────────────────────────────

  toggleEmojiDrawer(): void {
    this.showEmojiDrawer = !this.showEmojiDrawer;
  }

  insertEmoji(emoji: string): void {
    this.newMessage += emoji;
  }

  // ── IMAGE RESIZING USING HTML5 CANVAS ────────────────────────────────────────

  onFileSelected(event: any): void {
    const file = event.target.files[0] as File;
    if (!file) return;

    const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

    if (['.jpg', '.jpeg', '.png', '.webp', '.gif'].includes(extension)) {
      this.pendingImageFile = file;
      this.originalSize = file.size;

      // 5 MB Limit
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
    // Video Size Verification (Min 20 MB rule)
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
      alert('Unsupported file extension.');
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
    if (!this.resizedBlob || !this.pendingImageFile || !this.selectedSchedule || !this.currentUserId) return;

    const file = new File([this.resizedBlob], this.pendingImageFile.name, {
      type: 'image/jpeg'
    });

    this.chatService.uploadChatFile(file, 'image').subscribe({
      next: (res) => {
        this.chatService.sendMessageToSchedule(
          this.selectedSchedule.scheduleID,
          this.currentUserId!,
          this.currentUserFullName,
          `Sent Image (${this.getKBSize(this.resizedSize)})`,
          'image',
          res.fileUrl
        );
        this.cancelImagePending();
      }
    });
  }

  uploadDirectAttachment(file: File, type: string): void {
    if (!this.selectedSchedule || !this.currentUserId) return;
    this.chatService.uploadChatFile(file, type).subscribe({
      next: (res) => {
        this.chatService.sendMessageToSchedule(
          this.selectedSchedule.scheduleID,
          this.currentUserId!,
          this.currentUserFullName,
          file.name,
          type,
          res.fileUrl
        );
      }
    });
  }
}
