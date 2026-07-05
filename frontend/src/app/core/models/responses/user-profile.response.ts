import { BusinessIdeaSummaryResponse } from './business-idea.response';

export interface UserProfileResponse {
  id: string;
  displayName: string | null;
  avatarId: string | null;
  ideasCount: number;
  commentsCount: number;
  karma: number;
  wins: number;
  recentIdeas: BusinessIdeaSummaryResponse[];
}
