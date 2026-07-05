import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_BASE } from '../api.config';
import { VoteResultMapper } from '../mappers';
import { VoteDirection } from '../models/enums';
import { VoteResultResponse } from '../models/responses';
import { VoteResultViewModel } from '../models/view-models';

@Injectable({ providedIn: 'root' })
export class VotesApiService {
  private readonly baseUrl = API_BASE;

  constructor(private readonly http: HttpClient) {}

  voteOnIdea(postId: string, direction: VoteDirection): Observable<VoteResultViewModel> {
    return this.http
      .post<VoteResultResponse>(`${this.baseUrl}/ideas/${postId}/vote`, { direction })
      .pipe(map((response) => VoteResultMapper.toVoteResultViewModel(response)));
  }

  voteOnComment(commentId: string, direction: VoteDirection): Observable<VoteResultViewModel> {
    return this.http
      .post<VoteResultResponse>(`${this.baseUrl}/comments/${commentId}/vote`, { direction })
      .pipe(map((response) => VoteResultMapper.toVoteResultViewModel(response)));
  }
}
