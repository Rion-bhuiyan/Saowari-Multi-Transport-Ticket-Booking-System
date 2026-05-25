import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { animate, style, transition, trigger } from '@angular/animations';
import { NotificationService, Toast } from '../../../core/services/notification.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-toast-notification',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed bottom-4 right-4 z-[9999] flex flex-col gap-2">
      <div *ngFor="let toast of toasts$ | async" 
           @slideIn 
           class="alert shadow-lg flex-row min-w-[300px]"
           [ngClass]="{
             'alert-success': toast.type === 'success',
             'alert-error': toast.type === 'error',
             'alert-warning': toast.type === 'warning',
             'alert-info': toast.type === 'info'
           }">
        <div>
          <svg *ngIf="toast.type === 'success'" xmlns="http://www.w3.org/2000/svg" class="stroke-current flex-shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
          <svg *ngIf="toast.type === 'error'" xmlns="http://www.w3.org/2000/svg" class="stroke-current flex-shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
          <svg *ngIf="toast.type === 'warning'" xmlns="http://www.w3.org/2000/svg" class="stroke-current flex-shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>
          <svg *ngIf="toast.type === 'info'" xmlns="http://www.w3.org/2000/svg" class="stroke-current flex-shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
          <div>
            <h3 class="font-bold" *ngIf="toast.title">{{ toast.title }}</h3>
            <div class="text-sm">{{ toast.message }}</div>
          </div>
        </div>
        <button class="btn btn-sm btn-ghost btn-circle ml-auto" (click)="remove(toast.id)">✕</button>
      </div>
    </div>
  `,
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
      ])
    ])
  ]
})
export class ToastNotificationComponent implements OnInit {
  toasts$!: Observable<Toast[]>;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.toasts$ = this.notificationService.toasts$;
  }

  remove(id: string): void {
    this.notificationService.remove(id);
  }
}
