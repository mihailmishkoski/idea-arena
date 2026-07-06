import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, takeUntil } from 'rxjs/operators';

import {
  AVATARS,
  AuthService,
  AvatarViewModel,
  ChatService,
  CurrentUserViewModel,
  NotificationType,
  NotificationViewModel,
  NotificationsService,
} from '@core';

import { HeaderMenu } from '../view-models';

/**
 * Top navigation bar: brand, debounced live search, messages icon, notification
 * bell and the avatar menu (with the avatar picker). Badge counts update in
 * real time through the notification/chat stores.
 */
@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class HeaderComponent implements OnInit, OnDestroy {

  readonly currentUser$: Observable<CurrentUserViewModel | null> = this.auth.currentUser$;
  readonly notifications$: Observable<NotificationViewModel[]> = this.notifications.notifications$;
  readonly unreadNotifications$: Observable<number> = this.notifications.unreadCount$;
  readonly unreadMessages$: Observable<number> = this.chat.unreadTotal$;

  readonly avatars: AvatarViewModel[] = AVATARS;

  searchTerm = '';
  openMenu: HeaderMenu = null;
  changingAvatar = false;

  
  isAuthPage = false;

  private readonly minChars = 3;
  private readonly searchInput$ = new Subject<string>();
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly auth: AuthService,
    private readonly notifications: NotificationsService,
    private readonly chat: ChatService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {

    this.searchInput$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((term) => this.runSearch(term));

    // Hide search bar on auth pages
    this.isAuthPage = this.router.url.startsWith('/auth');

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.isAuthPage = this.router.url.startsWith('/auth');
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // --- Search ---------------------------------------------------------------

  onInput(term: string): void {
    this.searchInput$.next(term);
  }

  onSubmit(): void {
    this.runSearch(this.searchTerm);
  }

  // --- Menus ----------------------------------------------------------------

  toggleMenu(menu: Exclude<HeaderMenu, null>): void {
    const wasOpen = this.openMenu === menu;
    this.openMenu = wasOpen ? null : menu;
    this.changingAvatar = false;

    if (!wasOpen && menu === 'notifications') {
      this.notifications.markAllRead().subscribe();
    }
  }

  closeMenus(): void {
    this.openMenu = null;
    this.changingAvatar = false;
  }

  openNotification(notification: NotificationViewModel): void {
    this.closeMenus();

    if (!notification.targetId) {
      return;
    }

    const isChat =
      notification.type === NotificationType.ChatRequest ||
      notification.type === NotificationType.ChatAccepted ||
      notification.type === NotificationType.NewMessage;

    this.router.navigate(
      isChat
        ? ['/messages', notification.targetId]
        : ['/ideas', notification.targetId]
    );
  }

  goToMessages(): void {
    this.closeMenus();
    this.router.navigate(['/messages']);
  }

  goToProfile(userId: string): void {
    this.closeMenus();
    this.router.navigate(['/users', userId]);
  }

  // --- Avatar ---------------------------------------------------------------

  pickAvatar(avatarId: string): void {
    this.auth
      .updateAvatar(avatarId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => (this.changingAvatar = false));
  }

  onLogout(): void {
    this.closeMenus();
    this.auth.logout().subscribe(() => this.router.navigate(['/']));
  }

  private runSearch(term: string): void {
    const q = term.trim();

    if (q.length === 0) {
      this.router.navigate(['/']);
      return;
    }

    if (q.length < this.minChars) {
      return;
    }

    this.router.navigate(['/'], { queryParams: { q } });
  }
}