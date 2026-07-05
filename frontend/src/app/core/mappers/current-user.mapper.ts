import { CurrentUserResponse } from '../models/responses';
import { CurrentUserViewModel } from '../models/view-models';

export namespace CurrentUserMapper {
  export function toCurrentUserViewModel(response: CurrentUserResponse): CurrentUserViewModel {
    return {
      id: response.id,
      email: response.email,
      displayName: response.displayName,
      avatarId: response.avatarId,
    };
  }
}
