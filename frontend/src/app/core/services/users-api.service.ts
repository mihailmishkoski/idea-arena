import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { UserProfileMapper } from '../mappers';
import { UserProfileResponse } from '../models/responses';
import { UserProfileViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly baseUrl = `${API_BASE}/users`;

  constructor(private readonly http: HttpClient) {}

  getProfile(userId: string): Observable<UserProfileViewModel> {
    return this.http
      .get<UserProfileResponse>(`${this.baseUrl}/${encodeURIComponent(userId)}`)
      .pipe(map((response) => UserProfileMapper.toUserProfileViewModel(response)));
  }
}
