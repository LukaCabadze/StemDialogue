import { Signal, computed, linkedSignal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

import { PagedResult } from '../models';

export interface PagedResourceOptions<T, Q> {
  /** Reactive query; any change restarts from page 1. Return undefined to pause loading. */
  readonly query: () => Q | undefined;
  readonly load: (query: Q, page: number) => Observable<PagedResult<T>>;
}

export interface PagedResource<T> {
  /** Items of every page loaded so far for the current query. */
  readonly items: Signal<readonly T[]>;
  readonly total: Signal<number>;
  readonly hasMore: Signal<boolean>;
  readonly isLoading: Signal<boolean>;
  /** True only while the first page of a new query is loading. */
  readonly isFirstLoad: Signal<boolean>;
  readonly error: Signal<unknown>;
  readonly loadMore: () => void;
  readonly reload: () => void;
}

/**
 * "Show more" pagination on top of `rxResource`: keeps appending pages for the same query and
 * starts over when the query changes. Must be created in an injection context.
 */
export function pagedResource<T, Q>(options: PagedResourceOptions<T, Q>): PagedResource<T> {
  const page = linkedSignal({ source: options.query, computation: () => 1 });

  const resource = rxResource({
    params: () => {
      const query = options.query();
      return query === undefined ? undefined : { query, page: page() };
    },
    stream: ({ params }) => options.load(params.query, params.page),
  });

  const latest = computed(() => (resource.hasValue() ? resource.value() : undefined));

  const items = linkedSignal<PagedResult<T> | undefined, readonly T[]>({
    source: latest,
    computation: (result, previous) => {
      if (!result) {
        return previous?.value ?? [];
      }
      return result.page <= 1 ? result.items : [...(previous?.value ?? []), ...result.items];
    },
  });

  return {
    items: items.asReadonly(),
    total: computed(() => latest()?.total ?? 0),
    hasMore: computed(() => latest()?.hasMore ?? false),
    isLoading: resource.isLoading,
    isFirstLoad: computed(() => resource.isLoading() && page() === 1),
    error: resource.error,
    loadMore: () => {
      if (!resource.isLoading() && latest()?.hasMore) {
        page.update((current) => current + 1);
      }
    },
    reload: () => resource.reload(),
  };
}
