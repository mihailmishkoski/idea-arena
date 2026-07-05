import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { NotificationMapper } from '../mappers';
import { NotificationResponse } from '../models/responses';
import { NotificationViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly baseUrl = `${API_BASE}/notifications`;

  private readonly notificationsSubject = new BehaviorSubject<NotificationViewModel[]>([]);
  readonly notifications$ = this.notificationsSubject.asObservable();
  readonly unreadCount$ = this.notifications$.pipe(
    map((list) => list.filter((n) => !n.isRead).length)
  );

  constructor(private readonly http: HttpClient) {}

  load(): Observable<NotificationViewModel[]> {
    return this.http.get<NotificationResponse[]>(this.baseUrl).pipe(
      map((responses) =>
        responses.map((response) => NotificationMapper.toNotificationViewModel(response))
      ),
      tap((list) => this.notificationsSubject.next(list))
    );
  }

  receive(notification: NotificationViewModel): void {
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
