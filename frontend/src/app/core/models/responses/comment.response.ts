import { IdeaMetric, VoteDirection } from '../enums';

export interface CommentResponse {
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
