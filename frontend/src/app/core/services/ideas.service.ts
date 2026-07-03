import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { IdeaSortOrder } from '../models/enums';
import {
  BusinessIdeaDetail,
  BusinessIdeaSummary,
  CreateIdeaRequest,
} from '../models/business-idea.model';
import { PaginatedList } from '../models/paginated-list.model';

@Injectable({ providedIn: 'root' })
export class IdeasService {
  private readonly baseUrl = `${API_BASE}/ideas`;

  constructor(private readonly http: HttpClient) {}

  getIdeas(
    sortBy: IdeaSortOrder = IdeaSortOrder.Top,
    pageNumber = 1,
    pageSize = 20,
    search?: string | null
  ): Observable<PaginatedList<BusinessIdeaSummary>> {
    let params = new HttpParams()
      .set('sortBy', sortBy)
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PaginatedList<BusinessIdeaSummary>>(this.baseUrl, { params });
  }

  getIdea(id: string): Observable<BusinessIdeaDetail> {
    return this.http.get<BusinessIdeaDetail>(`${this.baseUrl}/${id}`);
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
