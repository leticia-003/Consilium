import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export type NotificationType = 'success' | 'error' | 'info';

export interface Notification {
  id: string;
  type: NotificationType;
  message: string;
  duration?: number; // ms
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private subject = new Subject<Notification>();

  get notifications(): Observable<Notification> {
    return this.subject.asObservable();
  }

  show(message: string, type: NotificationType = 'info', duration = 4000) {
    const note: Notification = { id: Math.random().toString(36).slice(2), type, message, duration };
    this.subject.next(note);
    return note.id;
  }

  showSuccess(message: string, duration = 4000) {
    return this.show(message, 'success', duration);
  }

  showError(message: string, duration = 6000) {
    return this.show(message, 'error', duration);
  }
}
