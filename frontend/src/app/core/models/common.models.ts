import { MediaKind } from './enums';

/** ISO-8601 timestamp with offset, e.g. `2026-11-21T12:00:00+04:00`. */
export type IsoDateTime = string;
/** ISO-8601 calendar date, e.g. `2026-10-01`. */
export type IsoDate = string;

export interface MediaDto {
  readonly id: string;
  readonly url: string;
  readonly alt: string | null;
  readonly caption: string | null;
  readonly credit: string | null;
  readonly width: number | null;
  readonly height: number | null;
  readonly contentType: string;
  readonly sizeBytes: number;
  readonly kind: MediaKind;
}

export interface SeoDto {
  readonly title: string;
  readonly description: string;
  readonly imageUrl: string | null;
  readonly canonicalPath: string;
}

export interface TermDto {
  readonly slug: string;
  readonly name: string;
}

export interface LabeledValue {
  readonly value: string;
  readonly label: string;
}

export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly total: number;
  readonly page: number;
  readonly pageSize: number;
  readonly totalPages: number;
  readonly hasMore: boolean;
}

export interface PageQuery {
  readonly page?: number;
  readonly pageSize?: number;
}
