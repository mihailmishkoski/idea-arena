import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { BusinessIdeaMapper, PaginatedListMapper } from '../mappers';
import { IdeaSortOrder } from '../models/enums';
import { CreateIdeaRequest } from '../models/requests';
import { BusinessIdeaDetailResponse, BusinessIdeaSummaryResponse, PaginatedListResponse } from '../models/responses';
import { BusinessIdeaDetailViewModel, BusinessIdeaSummaryViewModel, PaginatedListViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class IdeasApiService {
  private readonly baseUrl = `${API_BASE}/ideas`;

  constructor(private readonly http: HttpClient) {}

  getIdeas(
    sortBy: IdeaSortOrder = IdeaSortOrder.Top,
    pageNumber = 1,
    pageSize = 20,
    search?: string | null
  ): Observable<PaginatedListViewModel<BusinessIdeaSummaryViewModel>> {
    let params = new HttpParams()
      .set('sortBy', sortBy)
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http
      .get<PaginatedListResponse<BusinessIdeaSummaryResponse>>(this.baseUrl, { params })
      .pipe(
        map((response) =>
          PaginatedListMapper.toPaginatedListViewModel(response, (idea) =>
            BusinessIdeaMapper.toBusinessIdeaSummaryViewModel(idea)
          )
        )
      );
  }

  getIdea(id: string): Observable<BusinessIdeaDetailViewModel> {
    return this.http
      .get<BusinessIdeaDetailResponse>(`${this.baseUrl}/${id}`)
      .pipe(map((response) => BusinessIdeaMapper.toBusinessIdeaDetailViewModel(response)));
  }

  createIdea(request: CreateIdeaRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  updateIdea(id: string, request: CreateIdeaRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, { id, ...request });
  }

  deleteIdea(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
