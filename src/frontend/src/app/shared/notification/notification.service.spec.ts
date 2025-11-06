import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { NotificationService, NotificationType } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        NotificationService,
        provideZonelessChangeDetection()
      ]
    });
    service = TestBed.inject(NotificationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should emit notification when show is called', (done) => {
    const message = 'Test notification';
    const type: NotificationType = 'info';

    service.notifications.subscribe((notification) => {
      expect(notification.message).toBe(message);
      expect(notification.type).toBe(type);
      expect(notification.id).toBeTruthy();
      expect(notification.duration).toBe(4000);
      done();
    });

    service.show(message, type);
  });

  it('should emit success notification', (done) => {
    const message = 'Success message';

    service.notifications.subscribe((notification) => {
      expect(notification.message).toBe(message);
      expect(notification.type).toBe('success');
      expect(notification.duration).toBe(4000);
      done();
    });

    service.showSuccess(message);
  });

  it('should emit error notification with longer duration', (done) => {
    const message = 'Error message';

    service.notifications.subscribe((notification) => {
      expect(notification.message).toBe(message);
      expect(notification.type).toBe('error');
      expect(notification.duration).toBe(6000);
      done();
    });

    service.showError(message);
  });

  it('should use custom duration', (done) => {
    const message = 'Custom duration message';
    const customDuration = 10000;

    service.notifications.subscribe((notification) => {
      expect(notification.duration).toBe(customDuration);
      done();
    });

    service.show(message, 'info', customDuration);
  });

  it('should generate unique ids for different notifications', () => {
    const id1 = service.show('Message 1', 'info');
    const id2 = service.show('Message 2', 'info');
    const id3 = service.show('Message 3', 'info');

    expect(id1).not.toBe(id2);
    expect(id2).not.toBe(id3);
    expect(id1).not.toBe(id3);
  });

  it('should emit multiple notifications in sequence', (done) => {
    const messages = ['First', 'Second', 'Third'];
    const receivedMessages: string[] = [];

    service.notifications.subscribe((notification) => {
      receivedMessages.push(notification.message);
      if (receivedMessages.length === 3) {
        expect(receivedMessages).toEqual(messages);
        done();
      }
    });

    messages.forEach(msg => service.show(msg, 'info'));
  });
});
