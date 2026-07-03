import {
  AfterViewInit,
  Component,
  ElementRef,
  NgZone,
  OnDestroy,
  OnInit,
  ViewChild,
  ChangeDetectionStrategy
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { distinctUntilChanged, finalize, map, takeUntil } from 'rxjs/operators';
import { BusinessIdeaSummary } from '../../../core/models/business-idea.model';
import { IdeaSortOrder, VoteDirection } from '../../../core/models/enums';
import { AuthService } from '../../../core/services/auth.service';
import { IdeasService } from '../../../core/services/ideas.service';
import { VotesService } from '../../../core/services/votes.service';

interface SortOption {
  value: IdeaSortOrder;
  label: string;
  icon: string;
  hint: string;
}

@Component({
    selector: 'app-idea-list',
    templateUrl: './idea-list.component.html',
    styleUrls: ['./idea-list.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class IdeaListComponent implements OnInit, AfterViewInit, OnDestroy {
  ideas: BusinessIdeaSummary[] = [];
  loading = false;
  loadingMore = false;
  error = false;
  hasNextPage = false;

  sort: IdeaSortOrder = IdeaSortOrder.Top;
  search = '';
  sortMenuOpen = false;

  readonly SortOrder = IdeaSortOrder;
  readonly sortOptions: SortOption[] = [
    { value: IdeaSortOrder.Best, label: 'Best', icon: '🏅', hint: 'Most engagement' },
    { value: IdeaSortOrder.Top, label: 'Top', icon: '🔝', hint: 'Highest score' },
    { value: IdeaSortOrder.New, label: 'New', icon: '🆕', hint: 'Freshly posted' },
    { value: IdeaSortOrder.Controversial, label: 'Controversial', icon: '⚡', hint: 'Most divisive' },
    { value: IdeaSortOrder.Old, label: 'Old', icon: '🕰️', hint: 'Oldest first' },
    { value: IdeaSortOrder.Winners, label: 'Winners', icon: '🏆', hint: 'Closed ideas' },
  ];

  private readonly pageSize = 10;
  private pageNumber = 0;
  private sentinelVisible = false;
  private observer?: IntersectionObserver;

  @ViewChild('sentinel') private sentinel?: ElementRef<HTMLElement>;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly ideasService: IdeasService,
    private readonly votesService: VotesService,
    private readonly auth: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly zone: NgZone
  ) {}

  ngOnInit(): void {
    // The header drives search via the `q` query param; react to it (and the
    // initial value) by reloading the feed.
    this.route.queryParamMap
      .pipe(
        map((params) => params.get('q') ?? ''),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((q) => {
        this.search = q;
        this.fetchData();
      });
  }

  ngAfterViewInit(): void {
    if (!this.sentinel) {
      return;
    }

    this.observer = new IntersectionObserver(
      (entries) =>
        this.zone.run(() => {
          this.sentinelVisible = entries[0].isIntersecting;
          if (this.sentinelVisible) {
            this.loadMore();
          }
        }),
      { root: null, rootMargin: '300px', threshold: 0 }
    );

    this.observer.observe(this.sentinel.nativeElement);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    this.destroy$.next();
    this.destroy$.complete();
  }

  get isWinnersView(): boolean {
    return this.sort === IdeaSortOrder.Winners;
  }

  get currentSort(): SortOption {
    return this.sortOptions.find((o) => o.value === this.sort) ?? this.sortOptions[1];
  }

  /** The overall #1 closed idea is the winner. */
  isWinner(index: number): boolean {
    return this.isWinnersView && index === 0;
  }

  toggleSortMenu(): void {
    this.sortMenuOpen = !this.sortMenuOpen;
  }

  setSort(sort: IdeaSortOrder): void {
    this.sortMenuOpen = false;
    if (this.sort === sort) {
      return;
    }
    this.sort = sort;
    this.fetchData();
  }

  retry(): void {
    this.fetchData();
  }

  onVote(idea: BusinessIdeaSummary, direction: VoteDirection): void {
    if (!this.auth.currentUser) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.votesService
      .voteOnIdea(idea.id, direction)
      .pipe(takeUntil(this.destroy$))
      .subscribe((result) => {
        idea.upVotes = result.upVotes;
        idea.downVotes = result.downVotes;
        idea.score = result.score;
        idea.currentUserVote = result.currentUserVote;
      });
  }

  trackById(_index: number, idea: BusinessIdeaSummary): string {
    return idea.id;
  }

  private fetchData(): void {
    this.loading = true;
    this.error = false;
    this.pageNumber = 0;
    this.hasNextPage = false;
    this.ideas = [];

    this.ideasService
      .getIdeas(this.sort, 1, this.pageSize, this.search)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.tryFill();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (result) => {
          this.ideas = [...result.items];
          this.pageNumber = result.pageNumber;
          this.hasNextPage = result.hasNextPage;
        },
        error: () => (this.error = true),
      });
  }

  private loadMore(): void {
    if (this.loading || this.loadingMore || !this.hasNextPage) {
      return;
    }

    this.loadingMore = true;
    this.ideasService
      .getIdeas(this.sort, this.pageNumber + 1, this.pageSize, this.search)
      .pipe(
        finalize(() => {
          this.loadingMore = false;
          this.tryFill();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (result) => {
          this.ideas = [...this.ideas, ...result.items];
          this.pageNumber = result.pageNumber;
          this.hasNextPage = result.hasNextPage;
        },
        error: () => (this.hasNextPage = false),
      });
  }

  private tryFill(): void {
    if (this.sentinelVisible && this.hasNextPage) {
      this.loadMore();
    }
  }
}
