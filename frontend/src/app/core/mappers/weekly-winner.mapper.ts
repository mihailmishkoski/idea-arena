import { WeeklyWinnerResponse } from '../models/responses';
import { WeeklyWinnerViewModel } from '../models/view-models';

export namespace WeeklyWinnerMapper {
  export function toWeeklyWinnerViewModel(response: WeeklyWinnerResponse): WeeklyWinnerViewModel {
    return {
      id: response.id,
      periodStartUtc: response.periodStartUtc,
      periodEndUtc: response.periodEndUtc,
      postId: response.postId,
      postName: response.postName,
      authorId: response.authorId,
      authorName: response.authorName,
      authorAvatar: response.authorAvatar,
      upVotes: response.upVotes,
      downVotes: response.downVotes,
      score: response.score,
      commentCount: response.commentCount,
    };
  }
}
