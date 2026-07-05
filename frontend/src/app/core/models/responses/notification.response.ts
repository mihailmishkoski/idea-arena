import { NotificationType } from '../enums';

export interface NotificationResponse {
  id: string;
  type: NotificationType;
  text: string;
  targetId: string | null;
  isRead: boolean;
  createdAtUtc: string;
}
