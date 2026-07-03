import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router } from '@angular/router';
import { BusinessIdeaSummary } from '../../../../core/models/business-idea.model';
import { VoteDirection } from '../../../../core/models/enums';
import { HOT_SCORE_THRESHOLD, ideaExpiresAt } from '../../../../core/idea-rules';

/**
 * A single idea row in the feed. The whole card is clickable; the vote rail
 * stops propagation. Active high-scoring ideas get a "hot" treatment, and the
 * top closed idea gets a winner badge.
 */
@Component({
    selector: 'app-idea-card',
    templateUrl: './idea-card.component.html',
    styleUrls: ['./idea-card.component.scss'],
    standalone: false
})
export class IdeaCardComponent {
  @Input() idea!: BusinessIdeaSummary;
  @Input() isWinner = false;
  @Input() closed = false;

  @Output() vote = new EventEmitter<VoteDirection>();

  constructor(private readonly router: Router) {}

  get isHot(): boolean {
    return !this.closed && this.idea.score >= HOT_SCORE_THRESHOLD;
  }

  /** Human countdown until the idea closes, or null when already closed. */
  get timeLeftLabel(): string | null {
    if (this.closed) {
      return null;
    }

    const msLeft = ideaExpiresAt(this.idea.createdAtUtc).getTime() - Date.now();
    if (msLeft <= 0) {
      return null;
    }

    const hours = Math.floor(msLeft / (60 * 60 * 1000));
    if (hours >= 24) {
      return `closes in ${Math.floor(hours / 24)}d`;
    }
    if (hours >= 1) {
      return `closes in ${hours}h`;
    }
    return 'closes soon';
  }

  openDetail(): void {
    this.router.navigate(['/ideas', this.idea.id]);
  }

  onVote(direction: VoteDirection): void {
    this.vote.emit(direction);
  }
}
