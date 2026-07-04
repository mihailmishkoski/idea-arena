/** One hall-of-fame entry: the winning idea of a competition week. */
export interface WeeklyWinner {
  id: string;
  periodStartUtc: string;
  periodEndUtc: string;
  /** Null when the winning post was deleted; snapshot fields still describe it. */
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
