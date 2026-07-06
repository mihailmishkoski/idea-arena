import { RegistrationResultResponse } from '../models/responses';
import { RegistrationResultViewModel } from '../models/view-models';

export namespace RegistrationResultMapper {
  export function toRegistrationResultViewModel(
    response: RegistrationResultResponse
  ): RegistrationResultViewModel {
    return {
      requiresConfirmation: response.requiresConfirmation,
      email: response.email,
    };
  }
}
