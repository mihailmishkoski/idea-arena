import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { PaginatedList } from '../models/paginated-list.model';
import { WeeklyWinner } from '../models/winner.model';

@Injectable({ providedIn: 'root' })
export class WinnersService {
  private readonly baseUrl = `${API_BASE}/winners`;

  constructor(private readonly http: HttpClient) {}

  getWinners(pageNumber = 1, pageSize = 10): Observable<PaginatedList<WeeklyWinner>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PaginatedList<WeeklyWinner>>(this.baseUrl, { params });
  }
}
