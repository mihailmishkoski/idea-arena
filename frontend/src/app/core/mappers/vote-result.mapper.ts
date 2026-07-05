import { VoteResultResponse } from '../models/responses';
import { VoteResultViewModel } from '../models/view-models';

export namespace VoteResultMapper {
  export function toVoteResultViewModel(response: VoteResultResponse): VoteResultViewModel {
    return {
      upVotes: response.upVotes,
      downVotes: response.downVotes,
      score: response.score,
      currentUserVote: response.currentUserVote,
    };
  }
}
