export enum ChatRequestStatus {
  Pending = 0,
  Accepted = 1,
  Declined = 2,
}

export interface ConversationDto {
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

export interface ChatMessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  sentAtUtc: string;
}
