import { NotificationResponse } from '../models/responses';
import { NotificationViewModel } from '../models/view-models';

export namespace NotificationMapper {
  export function toNotificationViewModel(response: NotificationResponse): NotificationViewModel {
    return {
      id: response.id,
      type: response.type,
      text: response.text,
      targetId: response.targetId,
      isRead: response.isRead,
      createdAtUtc: response.createdAtUtc,
    };
  }
}
