import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { CommentDto, CreateCommentRequest } from '../models/comment.model';
import { IdeaMetric } from '../models/enums';

@Injectable({ providedIn: 'root' })
export class CommentsService {
  private readonly baseUrl = API_BASE;

  constructor(private readonly http: HttpClient) {}

  getComments(postId: string, metric?: IdeaMetric): Observable<CommentDto[]> {
    let params = new HttpParams();
    if (metric !== undefined && metric !== null) {
      params = params.set('metric', metric);
    }

    return this.http.get<CommentDto[]>(`${this.baseUrl}/ideas/${postId}/comments`, { params });
  }

  createComment(postId: string, request: CreateCommentRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/ideas/${postId}/comments`, request);
  }

  deleteComment(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/comments/${commentId}`);
  }
}
