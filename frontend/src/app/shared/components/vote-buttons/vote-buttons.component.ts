import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { VoteDirection } from '@core';

/**
 * Reddit-style up/down vote control. Purely presentational: it renders the
 * current score and highlighted state, and emits the direction the user clicked.
 * The parent owns the actual voting call and state.
 */
@Component({
    selector: 'app-vote-buttons',
    templateUrl: './vote-buttons.component.html',
    styleUrls: ['./vote-buttons.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class VoteButtonsComponent {
  @Input() score = 0;
  @Input() currentUserVote: VoteDirection | null = null;
  @Input() orientation: 'vertical' | 'horizontal' = 'vertical';
  @Input() disabled = false;

  @Output() vote = new EventEmitter<VoteDirection>();

  readonly Direction = VoteDirection;

  get isUpvoted(): boolean {
    return this.currentUserVote === VoteDirection.Up;
  }

  get isDownvoted(): boolean {
    return this.currentUserVote === VoteDirection.Down;
  }

  onVote(direction: VoteDirection): void {
    if (this.disabled) {
      return;
    }
    this.vote.emit(direction);
  }
}
