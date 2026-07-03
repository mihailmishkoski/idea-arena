import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { AVATARS, AvatarInfo } from '../../core/avatars';
import { CurrentUser } from '../../core/models/user.model';
import { NotificationDto, NotificationType } from '../../core/models/notification.model';
import { AuthService } from '../../core/services/auth.service';
import { ChatService } from '../../core/services/chat.service';
import { NotificationsService } from '../../core/services/notifications.service';

type HeaderMenu = 'notifications' | 'avatar' | null;

/**
 * Top navigation bar: brand, debounced live search, messages icon, notification
 * bell and the avatar menu (with the avatar picker). Badge counts update in
 * real time through the notification/chat stores.
 */
@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss'],
    standalone: false
})
export class HeaderComponent implements OnInit, OnDestroy {
  readonly currentUser$: Observable<CurrentUser | null> = this.auth.currentUser$;
  readonly notifications$: Observable<NotificationDto[]> = this.notifications.notifications$;
  readonly unreadNotifications$: Observable<number> = this.notifications.unreadCount$;
  readonly unreadMessages$: Observable<number> = this.chat.unreadTotal$;

  readonly avatars: AvatarInfo[] = AVATARS;

  searchTerm = '';
  openMenu: HeaderMenu = null;
  changingAvatar = false;

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
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((term) => this.runSearch(term));
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

  // --- Menus ------------------------------------------------------------------

  toggleMenu(menu: Exclude<HeaderMenu, null>): void {
    const wasOpen = this.openMenu === menu;
    this.openMenu = wasOpen ? null : menu;
    this.changingAvatar = false;

    // Opening the bell marks everything as seen.
    if (!wasOpen && menu === 'notifications') {
      this.notifications.markAllRead().subscribe();
    }
  }

  closeMenus(): void {
    this.openMenu = null;
    this.changingAvatar = false;
  }

  openNotification(notification: NotificationDto): void {
    this.closeMenus();

    if (!notification.targetId) {
      return;
    }

    const isChat =
      notification.type === NotificationType.ChatRequest ||
      notification.type === NotificationType.ChatAccepted ||
      notification.type === NotificationType.NewMessage;

    this.router.navigate(isChat ? ['/messages', notification.targetId] : ['/ideas', notification.targetId]);
  }

  goToMessages(): void {
    this.closeMenus();
    this.router.navigate(['/messages']);
  }

  // --- Avatar -----------------------------------------------------------------

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
