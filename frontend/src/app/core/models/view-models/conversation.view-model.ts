import { ChatRequestStatus } from '../enums';

export interface ConversationViewModel {
  id: string;
  status: ChatRequestStatus;
  iAmRequester: boolean;
  otherUserId: string;
  otherUserName: string | null;
  otherUserAvatar: string | null;
  postId: string | null;
  postName: string | null;
  lastMessage: string | null;
  lastMessageAtUtc: string | null;
  createdAtUtc: string;
  unreadCount: number;
}
