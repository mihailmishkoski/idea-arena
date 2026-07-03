export enum NotificationType {
  PostComment = 0,
  CommentReply = 1,
  ChatRequest = 2,
  ChatAccepted = 3,
  NewMessage = 4,
}

export interface NotificationDto {
  id: string;
  type: NotificationType;
  text: string;
  targetId: string | null;
  isRead: boolean;
  createdAtUtc: string;
}
