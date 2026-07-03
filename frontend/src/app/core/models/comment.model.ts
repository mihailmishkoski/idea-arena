import { IdeaMetric, VoteDirection } from './enums';

export interface CommentDto {
  id: string;
  postId: string;
  parentCommentId: string | null;
  authorId: string;
  authorName: string | null;
  authorAvatar: string | null;
  content: string;
  targetMetric: IdeaMetric;
  createdAtUtc: string;
  upVotes: number;
  downVotes: number;
  score: number;
  currentUserVote: VoteDirection | null;
}

/** A comment plus its nested replies, built client-side for threaded display. */
export interface CommentNode extends CommentDto {
  replies: CommentNode[];
  depth: number;
}

export interface CreateCommentRequest {
  content: string;
  targetMetric: IdeaMetric;
  parentCommentId?: string | null;
}
