import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import {
  BusinessIdeaSummaryViewModel,
  BusinessIdeaCategory,
  HOT_SCORE_THRESHOLD,
  VoteDirection,
  ideaExpiresAt,
} from '@core';

@Component({
    selector: 'app-idea-card',
    templateUrl: './idea-card.component.html',
    styleUrls: ['./idea-card.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class IdeaCardComponent implements OnChanges {
  @Input() idea!: BusinessIdeaSummaryViewModel;
  @Input() isWinner = false;
  @Input() closed = false;

  @Output() vote = new EventEmitter<VoteDirection>();

  categoryLabels: string[] = [];

  constructor(private readonly router: Router) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['idea']) {
      this.categoryLabels = (this.idea.categories ?? []).map(c => BusinessIdeaCategory[c]);
    }
  }

  get isHot(): boolean {
    return !this.closed && this.idea.score >= HOT_SCORE_THRESHOLD;
  }

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
