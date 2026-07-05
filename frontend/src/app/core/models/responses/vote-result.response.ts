import { VoteDirection } from '../enums';

export interface VoteResultResponse {
  upVotes: number;
  downVotes: number;
  score: number;
  currentUserVote: VoteDirection | null;
}
