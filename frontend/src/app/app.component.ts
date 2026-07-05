import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService, ChatService, NotificationsService, RealtimeService } from '@core';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly auth: AuthService,
    private readonly realtime: RealtimeService,
    private readonly notifications: NotificationsService,
    private readonly chat: ChatService
  ) {}

  ngOnInit(): void {
    // Bring the realtime connection and the badge stores up and down with the session.
    this.auth.currentUser$.pipe(takeUntil(this.destroy$)).subscribe((user) => {
      if (user) {
        this.realtime.start();
        this.notifications.load().subscribe();
        this.chat.load().subscribe();
      } else {
        this.realtime.stop();
        this.notifications.clear();
        this.chat.clear();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.realtime.stop();
  }
}
