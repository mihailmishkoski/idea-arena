import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { PaginatedListMapper, WeeklyWinnerMapper } from '../mappers';
import { PaginatedListResponse, WeeklyWinnerResponse } from '../models/responses';
import { PaginatedListViewModel, WeeklyWinnerViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class WinnersApiService {
  private readonly baseUrl = `${API_BASE}/winners`;

  constructor(private readonly http: HttpClient) {}

  getWinners(pageNumber = 1, pageSize = 10): Observable<PaginatedListViewModel<WeeklyWinnerViewModel>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http
      .get<PaginatedListResponse<WeeklyWinnerResponse>>(this.baseUrl, { params })
      .pipe(
        map((response) =>
          PaginatedListMapper.toPaginatedListViewModel(response, (winner) =>
            WeeklyWinnerMapper.toWeeklyWinnerViewModel(winner)
          )
        )
      );
  }
}
