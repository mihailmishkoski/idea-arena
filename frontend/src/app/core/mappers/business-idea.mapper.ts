import { BusinessIdeaDetailResponse, BusinessIdeaSummaryResponse } from '../models/responses';
import { BusinessIdeaDetailViewModel, BusinessIdeaSummaryViewModel } from '../models/view-models';

export namespace BusinessIdeaMapper {
  export function toBusinessIdeaSummaryViewModel(
    response: BusinessIdeaSummaryResponse
  ): BusinessIdeaSummaryViewModel {
    return {
      id: response.id,
      name: response.name,
      uniqueValueProposition: response.uniqueValueProposition,
      categories: response.categories,
      authorId: response.authorId,
      authorName: response.authorName,
      authorAvatar: response.authorAvatar,
      createdAtUtc: response.createdAtUtc,
      upVotes: response.upVotes,
      downVotes: response.downVotes,
      score: response.score,
      commentCount: response.commentCount,
      currentUserVote: response.currentUserVote,
    };
  }

  export function toBusinessIdeaDetailViewModel(
    response: BusinessIdeaDetailResponse
  ): BusinessIdeaDetailViewModel {
    return {
      id: response.id,
      name: response.name,
      uniqueValueProposition: response.uniqueValueProposition,
      problem: response.problem,
      solution: response.solution,
      categories: response.categories,
      competition: response.competition,
      incomeStrategy: response.incomeStrategy,
      exitStrategy: response.exitStrategy,
      videoPitchUrl: response.videoPitchUrl,
      authorId: response.authorId,
      authorName: response.authorName,
      authorAvatar: response.authorAvatar,
      createdAtUtc: response.createdAtUtc,
      updatedAtUtc: response.updatedAtUtc,
      upVotes: response.upVotes,
      downVotes: response.downVotes,
      score: response.score,
      commentCount: response.commentCount,
      currentUserVote: response.currentUserVote,
    };
  }
}
