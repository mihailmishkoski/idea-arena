import { IdeaMetric, VoteDirection } from '../enums';

export interface CommentViewModel {
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

export interface CommentNodeViewModel extends CommentViewModel {
  replies: CommentNodeViewModel[];
  depth: number;
}

export interface CommentVoteEvent {
  comment: CommentViewModel;
  direction: VoteDirection;
}

export interface CommentReplyEvent {
  parent: CommentViewModel;
  content: string;
}
