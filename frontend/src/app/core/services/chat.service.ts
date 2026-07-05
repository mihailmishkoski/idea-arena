import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { ChatMessageMapper, ConversationMapper } from '../mappers';
import { ChatRequestStatus } from '../models/enums';
import { CofoundApplicationRequest } from '../models/requests';
import { ChatMessageResponse, ConversationResponse } from '../models/responses';
import { ChatMessageViewModel, ConversationViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly baseUrl = `${API_BASE}/chat`;

  private readonly conversationsSubject = new BehaviorSubject<ConversationViewModel[]>([]);
  readonly conversations$ = this.conversationsSubject.asObservable();

  private readonly incomingMessageSubject = new Subject<ChatMessageViewModel>();
  readonly incomingMessage$ = this.incomingMessageSubject.asObservable();

  readonly unreadTotal$ = this.conversations$.pipe(
    map((list) =>
      list.reduce((sum, c) => sum + c.unreadCount, 0) +
      list.filter((c) => c.status === ChatRequestStatus.Pending && !c.iAmRequester).length
    )
  );

  constructor(private readonly http: HttpClient) {}

  load(): Observable<ConversationViewModel[]> {
    return this.http.get<ConversationResponse[]>(`${this.baseUrl}/conversations`).pipe(
      map((responses) =>
        responses.map((response) => ConversationMapper.toConversationViewModel(response))
      ),
      tap((list) => this.conversationsSubject.next(list))
    );
  }

  requestChat(recipientId: string, postId?: string | null): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/requests`, { recipientId, postId });
  }

  applyCofound(application: CofoundApplicationRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/cofound`, application);
  }

  respond(conversationId: string, accept: boolean): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}/requests/${conversationId}/respond`, { accept })
      .pipe(
        tap(() =>
          this.patchConversation(conversationId, {
            status: accept ? ChatRequestStatus.Accepted : ChatRequestStatus.Declined,
          })
        )
      );
  }

  getMessages(conversationId: string): Observable<ChatMessageViewModel[]> {
    return this.http
      .get<ChatMessageResponse[]>(`${this.baseUrl}/conversations/${conversationId}/messages`)
      .pipe(
        map((responses) =>
          responses.map((response) => ChatMessageMapper.toChatMessageViewModel(response))
        ),
        tap(() => this.patchConversation(conversationId, { unreadCount: 0 }))
      );
  }

  sendMessage(conversationId: string, content: string): Observable<ChatMessageViewModel> {
    return this.http
      .post<ChatMessageResponse>(`${this.baseUrl}/conversations/${conversationId}/messages`, {
        content,
      })
      .pipe(
        map((response) => ChatMessageMapper.toChatMessageViewModel(response)),
        tap((message) =>
          this.patchConversation(conversationId, {
            lastMessage: message.content,
            lastMessageAtUtc: message.sentAtUtc,
          })
        )
      );
  }

  receiveMessage(message: ChatMessageViewModel): void {
    const conversation = this.conversationsSubject.value.find(
      (c) => c.id === message.conversationId
    );

    if (conversation) {
      this.patchConversation(message.conversationId, {
        lastMessage: message.content,
        lastMessageAtUtc: message.sentAtUtc,
        unreadCount: conversation.unreadCount + 1,
      });
    } else {
      this.load().subscribe();
    }

    this.incomingMessageSubject.next(message);
  }

  markReadLocally(conversationId: string): void {
    this.patchConversation(conversationId, { unreadCount: 0 });
  }

  clear(): void {
    this.conversationsSubject.next([]);
  }

  private patchConversation(id: string, patch: Partial<ConversationViewModel>): void {
    this.conversationsSubject.next(
      this.conversationsSubject.value.map((c) => (c.id === id ? { ...c, ...patch } : c))
    );
  }
}
