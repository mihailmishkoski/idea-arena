import { PaginatedListResponse } from '../models/responses';
import { PaginatedListViewModel } from '../models/view-models';

export namespace PaginatedListMapper {
  export function toPaginatedListViewModel<TResponse, TViewModel>(
    response: PaginatedListResponse<TResponse>,
    itemMapper: (item: TResponse) => TViewModel
  ): PaginatedListViewModel<TViewModel> {
    return {
      items: response.items.map(itemMapper),
      pageNumber: response.pageNumber,
      totalPages: response.totalPages,
      totalCount: response.totalCount,
      hasPreviousPage: response.hasPreviousPage,
      hasNextPage: response.hasNextPage,
    };
  }
}
