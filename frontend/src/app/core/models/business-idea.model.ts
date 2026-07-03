import { VoteDirection } from './enums';

export interface BusinessIdeaSummary {
  id: string;
  name: string;
  uniqueValueProposition: string;
  authorId: string;
  authorName: string | null;
  authorAvatar: string | null;
  createdAtUtc: string;
  upVotes: number;
  downVotes: number;
  score: number;
  commentCount: number;
  currentUserVote: VoteDirection | null;
}

export interface BusinessIdeaDetail {
  id: string;
  name: string;
  uniqueValueProposition: string;
  problem: string;
  solution: string;
  competition: string | null;
  incomeStrategy: string | null;
  exitStrategy: string | null;
  videoPitchUrl: string | null;
  authorId: string;
  authorName: string | null;
  authorAvatar: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  upVotes: number;
  downVotes: number;
  score: number;
  commentCount: number;
  currentUserVote: VoteDirection | null;
}

export interface CreateIdeaRequest {
  name: string;
  uniqueValueProposition: string;
  problem: string;
  solution: string;
  competition?: string | null;
  incomeStrategy?: string | null;
  exitStrategy?: string | null;
  videoPitchUrl?: string | null;
}
