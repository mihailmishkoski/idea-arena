import { ChatMessageResponse } from '../models/responses';
import { ChatMessageViewModel } from '../models/view-models';

export namespace ChatMessageMapper {
  export function toChatMessageViewModel(response: ChatMessageResponse): ChatMessageViewModel {
    return {
      id: response.id,
      conversationId: response.conversationId,
      senderId: response.senderId,
      content: response.content,
      sentAtUtc: response.sentAtUtc,
    };
  }
}
