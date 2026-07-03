import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { ChatMessageDto, ChatRequestStatus, ConversationDto } from '../models/chat.model';

/** Payload of a co-founder application; every pitch field is optional. */
export interface CofoundApplication {
  postId: string;
  role?: string | null;
  skills?: string | null;
  motivation?: string | null;
  availability?: string | null;
  contactLink?: string | null;
}

/**
 * Holds the user's conversations and streams incoming realtime messages.
 * The envelope badge combines unread messages and incoming pending requests.
 */
@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly baseUrl = `${API_BASE}/chat`;

  private readonly conversationsSubject = new BehaviorSubject<ConversationDto[]>([]);
  readonly conversations$ = this.conversationsSubject.asObservable();

  /** Fires for every message pushed over the WebSocket. */
  private readonly incomingMessageSubject = new Subject<ChatMessageDto>();
  readonly incomingMessage$ = this.incomingMessageSubject.asObservable();

  readonly unreadTotal$ = this.conversations$.pipe(
    map((list) =>
      list.reduce((sum, c) => sum + c.unreadCount, 0) +
      list.filter((c) => c.status === ChatRequestStatus.Pending && !c.iAmRequester).length
    )
  );

  constructor(private readonly http: HttpClient) {}

  load(): Observable<ConversationDto[]> {
    return this.http
      .get<ConversationDto[]>(`${this.baseUrl}/conversations`)
      .pipe(tap((list) => this.conversationsSubject.next(list)));
  }

  requestChat(recipientId: string, postId?: string | null): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/requests`, { recipientId, postId });
  }

  /** Submits a co-founder application; the fields become the first message. */
  applyCofound(application: CofoundApplication): Observable<string> {
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

  getMessages(conversationId: string): Observable<ChatMessageDto[]> {
    return this.http
      .get<ChatMessageDto[]>(`${this.baseUrl}/conversations/${conversationId}/messages`)
      .pipe(tap(() => this.patchConversation(conversationId, { unreadCount: 0 })));
  }

  sendMessage(conversationId: string, content: string): Observable<ChatMessageDto> {
    return this.http
      .post<ChatMessageDto>(`${this.baseUrl}/conversations/${conversationId}/messages`, { content })
      .pipe(
        tap((message) =>
          this.patchConversation(conversationId, {
            lastMessage: message.content,
            lastMessageAtUtc: message.sentAtUtc,
          })
        )
      );
  }

  /** Called by the realtime service when a chat message is pushed. */
  receiveMessage(message: ChatMessageDto): void {
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
      // A brand-new conversation (request accepted elsewhere) — refresh the list.
      this.load().subscribe();
    }

    this.incomingMessageSubject.next(message);
  }

  /** The active chat window marks a conversation read locally. */
  markReadLocally(conversationId: string): void {
    this.patchConversation(conversationId, { unreadCount: 0 });
  }

  clear(): void {
    this.conversationsSubject.next([]);
  }

  private patchConversation(id: string, patch: Partial<ConversationDto>): void {
    this.conversationsSubject.next(
      this.conversationsSubject.value.map((c) => (c.id === id ? { ...c, ...patch } : c))
    );
  }
}
