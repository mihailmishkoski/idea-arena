import { VoteDirection, BusinessIdeaCategory } from '../enums';

export interface BusinessIdeaSummaryViewModel {
  id: string;
  name: string;
  uniqueValueProposition: string;
  categories: BusinessIdeaCategory[];
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

export interface BusinessIdeaDetailViewModel {
  id: string;
  name: string;
  uniqueValueProposition: string;
  problem: string;
  solution: string;
  categories: BusinessIdeaCategory[];
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
