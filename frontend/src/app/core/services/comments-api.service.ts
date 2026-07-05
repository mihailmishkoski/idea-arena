import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { CommentMapper } from '../mappers';
import { IdeaMetric } from '../models/enums';
import { CreateCommentRequest } from '../models/requests';
import { CommentResponse } from '../models/responses';
import { CommentViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class CommentsApiService {
  private readonly baseUrl = API_BASE;

  constructor(private readonly http: HttpClient) {}

  getComments(postId: string, metric?: IdeaMetric): Observable<CommentViewModel[]> {
    let params = new HttpParams();
    if (metric !== undefined && metric !== null) {
      params = params.set('metric', metric);
    }

    return this.http
      .get<CommentResponse[]>(`${this.baseUrl}/ideas/${postId}/comments`, { params })
      .pipe(map((responses) => responses.map((response) => CommentMapper.toCommentViewModel(response))));
  }

  createComment(postId: string, request: CreateCommentRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/ideas/${postId}/comments`, request);
  }

  deleteComment(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/comments/${commentId}`);
  }
}
