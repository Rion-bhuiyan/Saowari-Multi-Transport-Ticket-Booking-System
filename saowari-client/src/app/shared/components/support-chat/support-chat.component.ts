import { Component, ElementRef, OnInit, ViewChild, AfterViewChecked, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ChatService, SupportMessage } from '../../../core/services/api/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { Subscription } from 'rxjs';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-support-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './support-chat.component.html',
  styleUrls: ['./support-chat.component.css']
})
export class SupportChatComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('messageContainer') private messageContainer!: ElementRef;
  @ViewChild('fileInput') private fileInput!: ElementRef;

  isOpen = false;
  messages: SupportMessage[] = [];
  newMessage = '';
  roomId?: number;
  userIdentity = 'Guest';
  userId?: number;
  adminName?: string;
  adminId?: number;

  // Notifications
  unreadCount = 0;
  showNewMessagePopup = false;
  private popupTimeout: any;

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

  // Voice Note Recorder
  mediaRecorder?: MediaRecorder;
  audioChunks: Blob[] = [];
  isRecording = false;
  recordingSeconds = 0;
  recordingInterval: any;

  // Theme Controller Mode
  isDarkMode = true;

  // Subscriptions
  private subs: Subscription[] = [];

  Math = Math;

  constructor(
    private chatService: ChatService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Resolve theme setting
    const savedTheme = localStorage.getItem('customer_chat_theme');
    this.isDarkMode = savedTheme ? savedTheme === 'dark' : true;

    // Resolve user state
    const currentUser = this.authService.currentUserValue;
    if (currentUser) {
      this.userIdentity = currentUser.email;
      this.userId = currentUser.userId || (currentUser as any).userID;
    } else {
      // Resolve client IP using helper or offline backup
      this.chatService.getClientIP().subscribe({
        next: (res) => {
          this.userIdentity = `Guest-${res.ip}`;
        },
        error: () => {
          this.userIdentity = `Guest-${Math.floor(1000 + Math.random() * 9000)}`;
        }
      });
    }

    // Connect to Hub
    this.chatService.startConnection();

    // Event Subscriptions
    this.subs.push(
      this.chatService.getConnectionStatus().subscribe(connected => {
        if (connected) {
          // Join support room as passenger/guest
          this.chatService.joinSupportRoom(this.userIdentity);
        }
      })
    );

    this.subs.push(
      this.chatService.roomJoined$.subscribe(id => {
        this.roomId = id;
        this.loadHistory();
      })
    );

    this.subs.push(
      this.chatService.receiveMessage$.subscribe(msg => {
        if (msg.roomId === this.roomId) {
          this.messages.push(msg);
          this.scrollToBottom();

          if (!this.isCurrentUser(msg.senderName) && !this.isOpen) {
            this.unreadCount++;
            this.showNewMessagePopup = true;
            
            if (this.popupTimeout) clearTimeout(this.popupTimeout);
            this.popupTimeout = setTimeout(() => {
              this.showNewMessagePopup = false;
              this.cdr.detectChanges();
            }, 6000);
          }
          this.cdr.detectChanges();
        }
      })
    );

    this.subs.push(
      this.chatService.adminPresence$.subscribe(data => {
        if (data.isPresent) {
          this.adminName = data.adminName;
          this.adminId = data.adminId;
        } else {
          this.adminName = undefined;
          this.adminId = undefined;
        }
      })
    );

  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.cleanupRecordingTimer();
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.unreadCount = 0;
      this.showNewMessagePopup = false;
      this.scrollToBottom();
    }
  }

  loadHistory(): void {
    if (!this.roomId) return;
    this.chatService.getRoomMessages(this.roomId).subscribe({
      next: (res) => {
        this.messages = res;
        this.scrollToBottom();
      }
    });
  }

  sendMessage(): void {
    if (!this.newMessage.trim() || !this.roomId) return;
    this.chatService.sendMessageToRoom(
      this.roomId,
      this.userIdentity,
      this.userId || null,
      this.newMessage,
      'text'
    );
    this.newMessage = '';
    this.showEmojiDrawer = false;
  }

  isCurrentUser(senderName: string): boolean {
    return senderName === this.userIdentity;
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

    // Image flows
    if (['.jpg', '.jpeg', '.png', '.webp', '.gif'].includes(extension)) {
      this.pendingImageFile = file;
      this.originalSize = file.size;

      // Check max image size (5 MB limit)
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
    // Video size verification (Minimum 20 MB rule)
    else if (['.mp4', '.mov', '.avi', '.mkv', '.webm'].includes(extension)) {
      if (file.size < 20 * 1024 * 1024) {
        alert('Validation Error: Video files must be at least 20 MB to be sent.');
        return;
      }
      this.uploadDirectAttachment(file, 'video');
    }
    // PDF / Word attachments
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

      // Draw resized image on canvas
      const width = img.width * this.resizeScale;
      const height = img.height * this.resizeScale;
      canvas.width = width;
      canvas.height = height;

      ctx.drawImage(img, 0, 0, width, height);

      // Compress canvas output to compressed Blob
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
    if (!this.resizedBlob || !this.pendingImageFile || !this.roomId) return;

    // Convert blob to File object
    const resizedFile = new File([this.resizedBlob], this.pendingImageFile.name, {
      type: 'image/jpeg'
    });

    this.chatService.uploadChatFile(resizedFile, 'image').subscribe({
      next: (res) => {
        this.chatService.sendMessageToRoom(
          this.roomId!,
          this.userIdentity,
          this.userId || null,
          `Sent Image (${this.getKBSize(this.resizedSize)})`,
          'image',
          res.fileUrl
        );
        this.cancelImagePending();
      },
      error: (err) => {
        alert(err.error || 'Failed to upload image.');
      }
    });
  }

  uploadDirectAttachment(file: File, type: string): void {
    if (!this.roomId) return;
    this.chatService.uploadChatFile(file, type).subscribe({
      next: (res) => {
        this.chatService.sendMessageToRoom(
          this.roomId!,
          this.userIdentity,
          this.userId || null,
          file.name,
          type,
          res.fileUrl
        );
      },
      error: (err) => {
        alert(err.error || 'Failed to upload attachment.');
      }
    });
  }

  // ── VOICE NOTE RECORDER ──────────────────────────────────────────────────────

  startRecording(): void {
    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
      this.audioChunks = [];
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.ondataavailable = (event) => {
        this.audioChunks.push(event.data);
      };
      
      this.mediaRecorder.onstop = () => {
        const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
        this.uploadVoiceNote(audioBlob);
      };

      this.mediaRecorder.start();
      this.isRecording = true;
      this.recordingSeconds = 0;
      this.recordingInterval = setInterval(() => {
        this.recordingSeconds++;
      }, 1000);
    }).catch(() => {
      alert('Microphone access denied. Cannot record voice notes.');
    });
  }

  stopRecording(): void {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
      this.isRecording = false;
      this.cleanupRecordingTimer();
    }
  }

  cancelRecording(): void {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
      this.isRecording = false;
      this.cleanupRecordingTimer();
      // Drop chunks
      this.audioChunks = [];
    }
  }

  cleanupRecordingTimer(): void {
    if (this.recordingInterval) {
      clearInterval(this.recordingInterval);
    }
  }

  uploadVoiceNote(blob: Blob): void {
    if (!this.roomId) return;
    const file = new File([blob], 'voicenote.webm', { type: 'audio/webm' });
    this.chatService.uploadChatFile(file, 'voice').subscribe({
      next: (res) => {
        this.chatService.sendMessageToRoom(
          this.roomId!,
          this.userIdentity,
          this.userId || null,
          'Voice Note Attachment',
          'voice',
          res.fileUrl
        );
      }
    });
  }

  // ── THEME SWITCHING DYNAMIC METHODS ───────────────────────────────────────

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    localStorage.setItem('customer_chat_theme', this.isDarkMode ? 'dark' : 'light');
  }
}
