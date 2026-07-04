import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { finalize, switchMap, takeUntil } from 'rxjs/operators';
import { UserProfile } from '../../../core/models/user-profile.model';
import { ideaExpiresAt } from '../../../core/idea-rules';
import { UsersService } from '../../../core/services/users.service';

/** A member's public profile: stats plus their recent ideas. */
@Component({
    selector: 'app-user-profile',
    templateUrl: './user-profile.component.html',
    styleUrls: ['./user-profile.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class UserProfileComponent implements OnInit, OnDestroy {
  profile: UserProfile | null = null;
  loading = false;
  error = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly usersService: UsersService
  ) {}

  ngOnInit(): void {
    // switchMap so navigating between profiles reuses the component.
    this.route.paramMap
      .pipe(
        switchMap((params) => {
          this.loading = true;
          this.error = false;
          this.profile = null;
          return this.usersService
            .getProfile(params.get('id') ?? '')
            .pipe(finalize(() => (this.loading = false)));
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (profile) => (this.profile = profile),
        error: () => (this.error = true),
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  isClosed(createdAtUtc: string): boolean {
    return ideaExpiresAt(createdAtUtc).getTime() <= Date.now();
  }
}
