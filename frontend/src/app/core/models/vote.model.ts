import { VoteDirection } from './enums';

/** Returned by the backend after a vote so the UI can refresh in place. */
export interface VoteResult {
  upVotes: number;
  downVotes: number;
  score: number;
  currentUserVote: VoteDirection | null;
}
