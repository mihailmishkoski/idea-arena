import { ConversationResponse } from '../models/responses';
import { ConversationViewModel } from '../models/view-models';

export namespace ConversationMapper {
  export function toConversationViewModel(response: ConversationResponse): ConversationViewModel {
    return {
      id: response.id,
      status: response.status,
      iAmRequester: response.iAmRequester,
      otherUserId: response.otherUserId,
      otherUserName: response.otherUserName,
      otherUserAvatar: response.otherUserAvatar,
      postId: response.postId,
      postName: response.postName,
      lastMessage: response.lastMessage,
      lastMessageAtUtc: response.lastMessageAtUtc,
      createdAtUtc: response.createdAtUtc,
      unreadCount: response.unreadCount,
    };
  }
}
