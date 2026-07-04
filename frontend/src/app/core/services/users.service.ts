import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { UserProfile } from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly baseUrl = `${API_BASE}/users`;

  constructor(private readonly http: HttpClient) {}

  getProfile(userId: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/${encodeURIComponent(userId)}`);
  }
}
