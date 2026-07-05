import { VoteDirection } from '../enums';

export interface VoteResultViewModel {
  upVotes: number;
  downVotes: number;
  score: number;
  currentUserVote: VoteDirection | null;
}
