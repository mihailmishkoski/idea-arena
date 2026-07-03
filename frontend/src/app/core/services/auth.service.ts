import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { CurrentUser, LoginRequest, RegisterRequest } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${API_BASE}/auth`;

  /** Single source of truth for the signed-in user, streamed to any subscriber. */
  private readonly currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly isAuthenticated$ = this.currentUser$.pipe(map((user) => user !== null));

  constructor(private readonly http: HttpClient) {}

  get currentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  /** Called once on app start to restore the session from the auth cookie. */
  loadCurrentUser(): Observable<CurrentUser | null> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/me`).pipe(
      tap((user) => this.currentUserSubject.next(user)),
      catchError(() => {
        this.currentUserSubject.next(null);
        return of(null);
      })
    );
  }

  login(request: LoginRequest): Observable<CurrentUser> {
    return this.http
      .post<CurrentUser>(`${this.baseUrl}/login`, request)
      .pipe(tap((user) => this.currentUserSubject.next(user)));
  }

  register(request: RegisterRequest): Observable<CurrentUser> {
    return this.http
      .post<CurrentUser>(`${this.baseUrl}/register`, request)
      .pipe(tap((user) => this.currentUserSubject.next(user)));
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}/logout`, {})
      .pipe(tap(() => this.currentUserSubject.next(null)));
  }

  updateAvatar(avatarId: string): Observable<CurrentUser> {
    return this.http
      .put<CurrentUser>(`${this.baseUrl}/avatar`, { avatarId })
      .pipe(tap((user) => this.currentUserSubject.next(user)));
  }
}
