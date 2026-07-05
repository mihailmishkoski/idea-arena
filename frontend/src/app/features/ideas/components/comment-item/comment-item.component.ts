import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import {
  CommentViewModel,
  CommentNodeViewModel,
  CommentReplyEvent,
  CommentVoteEvent,
} from '@core';
import { VoteDirection } from '@core';

/**
 * A single comment and, recursively, its replies. Presentational: it manages its
 * own inline reply box but delegates the actual create/vote/delete work to the
 * parent via events, which bubble up through each level of the thread.
 */
@Component({
    selector: 'app-comment-item',
    templateUrl: './comment-item.component.html',
    styleUrls: ['./comment-item.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class CommentItemComponent {
  @Input() comment!: CommentNodeViewModel;
  @Input() currentUserId: string | null = null;
  @Input() isAuthenticated = false;

  @Output() vote = new EventEmitter<CommentVoteEvent>();
  @Output() remove = new EventEmitter<CommentViewModel>();
  @Output() reply = new EventEmitter<CommentReplyEvent>();

  showReplyBox = false;
  replyText = '';

  get canDelete(): boolean {
    return this.comment.authorId === this.currentUserId;
  }

  get authorHandle(): string {
    return 'u/' + (this.comment.authorName || 'member');
  }

  toggleReplyBox(): void {
    this.showReplyBox = !this.showReplyBox;
  }

  submitReply(): void {
    const content = this.replyText.trim();
    if (!content) {
      return;
    }
    this.reply.emit({ parent: this.comment, content });
    this.replyText = '';
    this.showReplyBox = false;
  }

  onVote(direction: VoteDirection): void {
    this.vote.emit({ comment: this.comment, direction });
  }
}
