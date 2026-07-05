import { Injectable, NgZone } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { ChatMessageMapper, NotificationMapper } from '../mappers';
import { ChatMessageResponse } from '../models/responses';
import { NotificationResponse } from '../models/responses';
import { ChatService } from './chat.service';
import { NotificationsService } from './notifications.service';

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
      .withUrl('/hubs/chat')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('notification', (response: NotificationResponse) =>
      this.zone.run(() =>
        this.notifications.receive(NotificationMapper.toNotificationViewModel(response))
      )
    );

    this.connection.on('chatMessage', (response: ChatMessageResponse) =>
      this.zone.run(() =>
        this.chat.receiveMessage(ChatMessageMapper.toChatMessageViewModel(response))
      )
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
