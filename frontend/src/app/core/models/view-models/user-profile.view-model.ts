import { BusinessIdeaSummaryViewModel } from './business-idea.view-model';

export interface UserProfileViewModel {
  id: string;
  displayName: string | null;
  avatarId: string | null;
  ideasCount: number;
  commentsCount: number;
  karma: number;
  wins: number;
  recentIdeas: BusinessIdeaSummaryViewModel[];
}
