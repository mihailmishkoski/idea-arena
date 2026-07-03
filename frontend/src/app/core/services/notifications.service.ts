import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { NotificationDto } from '../models/notification.model';

/**
 * Holds the current user's notifications. Fed by HTTP on load and by the
 * realtime service when a push arrives, so the bell badge is always live.
 */
@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly baseUrl = `${API_BASE}/notifications`;

  private readonly notificationsSubject = new BehaviorSubject<NotificationDto[]>([]);
  readonly notifications$ = this.notificationsSubject.asObservable();
  readonly unreadCount$ = this.notifications$.pipe(
    map((list) => list.filter((n) => !n.isRead).length)
  );

  constructor(private readonly http: HttpClient) {}

  load(): Observable<NotificationDto[]> {
    return this.http
      .get<NotificationDto[]>(this.baseUrl)
      .pipe(tap((list) => this.notificationsSubject.next(list)));
  }

  /** Called by the realtime service when a notification is pushed. */
  receive(notification: NotificationDto): void {
    this.notificationsSubject.next([notification, ...this.notificationsSubject.value]);
  }

  markAllRead(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/mark-read`, {}).pipe(
      tap(() =>
        this.notificationsSubject.next(
          this.notificationsSubject.value.map((n) => ({ ...n, isRead: true }))
        )
      )
    );
  }

  clear(): void {
    this.notificationsSubject.next([]);
  }
}
