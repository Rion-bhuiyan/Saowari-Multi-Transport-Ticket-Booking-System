import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  message: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private toasts: Toast[] = [];
  public toasts$ = new BehaviorSubject<Toast[]>([]);

  constructor() { }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  show(toast: Omit<Toast, 'id'>): void {
    const newToast: Toast = {
      ...toast,
      id: this.generateId(),
      duration: toast.duration || 4000
    };

    this.toasts.push(newToast);
    if (this.toasts.length > 5) {
      this.toasts.shift();
    }
    this.toasts$.next([...this.toasts]);

    if (newToast.duration && newToast.duration > 0) {
      setTimeout(() => this.remove(newToast.id), newToast.duration);
    }
  }

  remove(id: string): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
    this.toasts$.next([...this.toasts]);
  }

  success(message: string, title?: string, duration?: number): void {
    this.show({ type: 'success', message, title, duration });
  }

  error(message: string, title?: string, duration?: number): void {
    this.show({ type: 'error', message, title, duration });
  }

  warning(message: string, title?: string, duration?: number): void {
    this.show({ type: 'warning', message, title, duration });
  }

  info(message: string, title?: string, duration?: number): void {
    this.show({ type: 'info', message, title, duration });
  }
}
