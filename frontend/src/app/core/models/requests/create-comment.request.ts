import { IdeaMetric } from '../enums';

export interface CreateCommentRequest {
  content: string;
  targetMetric: IdeaMetric;
  parentCommentId?: string | null;
}
