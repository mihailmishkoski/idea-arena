import { NotificationType } from '../enums';

export interface NotificationViewModel {
  id: string;
  type: NotificationType;
  text: string;
  targetId: string | null;
  isRead: boolean;
  createdAtUtc: string;
}
