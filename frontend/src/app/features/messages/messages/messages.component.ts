import { AfterViewChecked, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Subject } from 'rxjs';
import { finalize, map, takeUntil } from 'rxjs/operators';
import {
  ChatMessageDto,
  ChatRequestStatus,
  ConversationDto,
} from '../../../core/models/chat.model';
import { AuthService } from '../../../core/services/auth.service';
import { ChatService } from '../../../core/services/chat.service';

/**
 * The messaging screen: conversation list (incl. pending requests) on the left,
 * the active chat on the right. New messages arrive live over the WebSocket.
 */
@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss'],
})
export class MessagesComponent implements OnInit, AfterViewChecked, OnDestroy {
  conversations: ConversationDto[] = [];
  selected: ConversationDto | null = null;
  messages: ChatMessageDto[] = [];

  loadingMessages = false;
  sending = false;
  draft = '';
  currentUserId: string | null = null;

  readonly Status = ChatRequestStatus;

  @ViewChild('scroller') private scroller?: ElementRef<HTMLElement>;
  private shouldScroll = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly chat: ChatService,
    private readonly auth: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.auth.currentUser?.id ?? null;

    this.fetchData();

    // Keep the list and the selected conversation in sync with the store + URL.
    combineLatest([
      this.chat.conversations$,
      this.route.paramMap.pipe(map((params) => params.get('id'))),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([conversations, id]) => {
        this.conversations = conversations;

        const target = id ? conversations.find((c) => c.id === id) ?? null : null;
        if (target && target.id !== this.selected?.id) {
          this.selected = target;
          this.fetchMessages(target.id);
        } else if (!id) {
          this.selected = null;
          this.messages = [];
        } else if (target) {
          this.selected = target; // refresh reference (status changes etc.)
        }
      });

    // Live incoming messages: append when the open chat matches, then mark read.
    this.chat.incomingMessage$
      .pipe(takeUntil(this.destroy$))
      .subscribe((message) => {
        if (this.selected && message.conversationId === this.selected.id) {
          this.messages = [...this.messages, message];
          this.chat.markReadLocally(this.selected.id);
          this.shouldScroll = true;
        }
      });
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll && this.scroller) {
      this.scroller.nativeElement.scrollTop = this.scroller.nativeElement.scrollHeight;
      this.shouldScroll = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get pendingIncoming(): ConversationDto[] {
    return this.conversations.filter(
      (c) => c.status === ChatRequestStatus.Pending && !c.iAmRequester
    );
  }

  get regularConversations(): ConversationDto[] {
    return this.conversations.filter(
      (c) => !(c.status === ChatRequestStatus.Pending && !c.iAmRequester)
    );
  }

  isMine(message: ChatMessageDto): boolean {
    return message.senderId === this.currentUserId;
  }

  select(conversation: ConversationDto): void {
    this.router.navigate(['/messages', conversation.id]);
  }

  respond(conversation: ConversationDto, accept: boolean, event: Event): void {
    event.stopPropagation();
    this.chat
      .respond(conversation.id, accept)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (accept) {
          this.router.navigate(['/messages', conversation.id]);
        }
      });
  }

  send(): void {
    const content = this.draft.trim();
    if (!content || !this.selected || this.sending) {
      return;
    }

    this.sending = true;
    this.chat
      .sendMessage(this.selected.id, content)
      .pipe(
        finalize(() => (this.sending = false)),
        takeUntil(this.destroy$)
      )
      .subscribe((message) => {
        this.messages = [...this.messages, message];
        this.draft = '';
        this.shouldScroll = true;
      });
  }

  trackByMessageId(_index: number, message: ChatMessageDto): string {
    return message.id;
  }

  trackByConversationId(_index: number, conversation: ConversationDto): string {
    return conversation.id;
  }

  private fetchData(): void {
    this.chat.load().pipe(takeUntil(this.destroy$)).subscribe();
  }

  private fetchMessages(conversationId: string): void {
    this.loadingMessages = true;
    this.messages = [];

    this.chat
      .getMessages(conversationId)
      .pipe(
        finalize(() => (this.loadingMessages = false)),
        takeUntil(this.destroy$)
      )
      .subscribe((messages) => {
        this.messages = messages;
        this.shouldScroll = true;
      });
  }
}
