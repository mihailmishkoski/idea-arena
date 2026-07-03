import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { VoteDirection } from '../models/enums';
import { VoteResult } from '../models/vote.model';

@Injectable({ providedIn: 'root' })
export class VotesService {
  private readonly baseUrl = API_BASE;

  constructor(private readonly http: HttpClient) {}

  voteOnIdea(postId: string, direction: VoteDirection): Observable<VoteResult> {
    return this.http.post<VoteResult>(`${this.baseUrl}/ideas/${postId}/vote`, { direction });
  }

  voteOnComment(commentId: string, direction: VoteDirection): Observable<VoteResult> {
    return this.http.post<VoteResult>(`${this.baseUrl}/comments/${commentId}/vote`, { direction });
  }
}
