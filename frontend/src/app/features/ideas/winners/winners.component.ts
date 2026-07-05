import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { WeeklyWinnerViewModel, WinnersApiService } from '@core';

/**
 * The Hall of Fame: one entry per competition week, newest first. Winners are
 * snapshots — they stay on this page even if the original post was deleted.
 */
@Component({
    selector: 'app-winners',
    templateUrl: './winners.component.html',
    styleUrls: ['./winners.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class WinnersComponent implements OnInit, OnDestroy {
  winners: WeeklyWinnerViewModel[] = [];
  loading = false;
  hasNextPage = false;

  private pageNumber = 1;
  private readonly destroy$ = new Subject<void>();

  constructor(private readonly winnersService: WinnersApiService) {}

  ngOnInit(): void {
    this.fetchPage();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMore(): void {
    if (this.loading || !this.hasNextPage) {
      return;
    }
    this.pageNumber++;
    this.fetchPage();
  }

  trackByWinnerId(_index: number, winner: WeeklyWinnerViewModel): string {
    return winner.id;
  }

  private fetchPage(): void {
    this.loading = true;
    this.winnersService
      .getWinners(this.pageNumber)
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntil(this.destroy$)
      )
      .subscribe((page) => {
        this.winners = [...this.winners, ...page.items];
        this.hasNextPage = page.hasNextPage;
      });
  }
}
