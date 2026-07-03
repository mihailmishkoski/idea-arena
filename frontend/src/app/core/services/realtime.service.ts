import { Injectable, NgZone } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { ChatMessageDto } from '../models/chat.model';
import { NotificationDto } from '../models/notification.model';
import { ChatService } from './chat.service';
import { NotificationsService } from './notifications.service';

/**
 * Owns the WebSocket (SignalR) connection to the backend hub. Started when a
 * user signs in, stopped on sign-out. Incoming events are dispatched to the
 * notification and chat stores inside the Angular zone so the UI updates.
 */
@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private connection: HubConnection | null = null;

  constructor(
    private readonly notifications: NotificationsService,
    private readonly chat: ChatService,
    private readonly zone: NgZone
  ) {}

  start(): void {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      return;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl('/hubs/chat') // same-origin via the dev proxy; auth cookie flows along
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('notification', (dto: NotificationDto) =>
      this.zone.run(() => this.notifications.receive(dto))
    );

    this.connection.on('chatMessage', (dto: ChatMessageDto) =>
      this.zone.run(() => this.chat.receiveMessage(dto))
    );

    this.connection
      .start()
      .catch((err) => console.warn('Realtime connection failed to start:', err));
  }

  stop(): void {
    this.connection?.stop().catch(() => undefined);
    this.connection = null;
  }
}
