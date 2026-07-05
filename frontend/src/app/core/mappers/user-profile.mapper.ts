import { UserProfileResponse } from '../models/responses';
import { UserProfileViewModel } from '../models/view-models';
import { BusinessIdeaMapper } from './business-idea.mapper';

export namespace UserProfileMapper {
  export function toUserProfileViewModel(response: UserProfileResponse): UserProfileViewModel {
    return {
      id: response.id,
      displayName: response.displayName,
      avatarId: response.avatarId,
      ideasCount: response.ideasCount,
      commentsCount: response.commentsCount,
      karma: response.karma,
      wins: response.wins,
      recentIdeas: response.recentIdeas.map((idea) =>
        BusinessIdeaMapper.toBusinessIdeaSummaryViewModel(idea)
      ),
    };
  }
}
