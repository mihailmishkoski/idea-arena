import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { CurrentUserMapper } from '../mappers';
import { LoginRequest, RegisterRequest } from '../models/requests';
import { CurrentUserResponse } from '../models/responses';
import { CurrentUserViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${API_BASE}/auth`;

  private readonly currentUserSubject = new BehaviorSubject<CurrentUserViewModel | null>(null);
  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly isAuthenticated$ = this.currentUser$.pipe(map((user) => user !== null));

  constructor(private readonly http: HttpClient) {}

  get currentUser(): CurrentUserViewModel | null {
    return this.currentUserSubject.value;
  }

  loadCurrentUser(): Observable<CurrentUserViewModel | null> {
    return this.http.get<CurrentUserResponse>(`${this.baseUrl}/me`).pipe(
      map((response) => CurrentUserMapper.toCurrentUserViewModel(response)),
      tap((user) => this.currentUserSubject.next(user)),
      catchError(() => {
        this.currentUserSubject.next(null);
        return of(null);
      })
    );
  }

  login(request: LoginRequest): Observable<CurrentUserViewModel> {
    return this.http.post<CurrentUserResponse>(`${this.baseUrl}/login`, request).pipe(
      map((response) => CurrentUserMapper.toCurrentUserViewModel(response)),
      tap((user) => this.currentUserSubject.next(user))
    );
  }

  register(request: RegisterRequest): Observable<CurrentUserViewModel> {
    return this.http.post<CurrentUserResponse>(`${this.baseUrl}/register`, request).pipe(
      map((response) => CurrentUserMapper.toCurrentUserViewModel(response)),
      tap((user) => this.currentUserSubject.next(user))
    );
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}/logout`, {})
      .pipe(tap(() => this.currentUserSubject.next(null)));
  }

  updateAvatar(avatarId: string): Observable<CurrentUserViewModel> {
    return this.http.put<CurrentUserResponse>(`${this.baseUrl}/avatar`, { avatarId }).pipe(
      map((response) => CurrentUserMapper.toCurrentUserViewModel(response)),
      tap((user) => this.currentUserSubject.next(user))
    );
  }
}
