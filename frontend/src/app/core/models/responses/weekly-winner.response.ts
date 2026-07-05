export interface WeeklyWinnerResponse {
  id: string;
  periodStartUtc: string;
  periodEndUtc: string;
  postId: string | null;
  postName: string;
  authorId: string;
  authorName: string | null;
  authorAvatar: string | null;
  upVotes: number;
  downVotes: number;
  score: number;
  commentCount: number;
}
