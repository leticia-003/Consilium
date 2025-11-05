import { Component, OnDestroy, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NotificationService, Notification } from './notification.service';

type Toast = Notification & { closing?: boolean };

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.css']
})
export class NotificationComponent implements OnDestroy {
  notifications: Toast[] = [];
  private sub: Subscription;
  private ANIM_MS = 260;

  constructor(private svc: NotificationService, private zone: NgZone, private cdr: ChangeDetectorRef) {
    this.sub = this.svc.notifications.subscribe(n => this.push(n));
  }

  push(n: Notification) {
    const t: Toast = { ...n, closing: false };
    this.notifications.push(t);
    const duration = n.duration ?? 4000;

    this.zone.run(() => {
      setTimeout(() => {
        this.startClose(t.id);
        this.cdr.detectChanges();
        setTimeout(() => {
          this.finalRemove(t.id);
          this.cdr.detectChanges();
        }, this.ANIM_MS);
      }, duration);
    });
  }

  startClose(id: string) {
    const found = this.notifications.find(x => x.id === id);
    if (found) found.closing = true;
  }

  finalRemove(id: string) {
    this.notifications = this.notifications.filter(x => x.id !== id);
  }

  remove(id: string) {
    this.startClose(id);
    this.cdr.detectChanges();
    setTimeout(() => {
      this.finalRemove(id);
      this.cdr.detectChanges();
    }, this.ANIM_MS);
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}
