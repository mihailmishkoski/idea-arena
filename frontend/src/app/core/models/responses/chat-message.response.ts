export interface ChatMessageResponse {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  sentAtUtc: string;
}
