import { BusinessIdeaSummary } from './business-idea.model';

/** A member's public profile: identity basics plus community stats. */
export interface UserProfile {
  id: string;
  displayName: string | null;
  avatarId: string | null;
  ideasCount: number;
  commentsCount: number;
  /** Net votes received across the member's ideas and comments. */
  karma: number;
  /** Competition weeks won. */
  wins: number;
  recentIdeas: BusinessIdeaSummary[];
}
