import { CommentResponse } from '../models/responses';
import { CommentViewModel } from '../models/view-models';

export namespace CommentMapper {
  export function toCommentViewModel(response: CommentResponse): CommentViewModel {
    return {
      id: response.id,
      postId: response.postId,
      parentCommentId: response.parentCommentId,
      authorId: response.authorId,
      authorName: response.authorName,
      authorAvatar: response.authorAvatar,
      content: response.content,
      targetMetric: response.targetMetric,
      createdAtUtc: response.createdAtUtc,
      upVotes: response.upVotes,
      downVotes: response.downVotes,
      score: response.score,
      currentUserVote: response.currentUserVote,
    };
  }
}
