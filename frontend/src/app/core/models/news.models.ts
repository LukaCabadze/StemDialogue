import { IsoDateTime, MediaDto, SeoDto, TermDto } from './common.models';
import { NewsCategory } from './enums';
import { EventCardDto } from './event.models';

export interface NewsCardDto {
  readonly slug: string;
  readonly title: string;
  readonly summary: string;
  readonly category: NewsCategory;
  readonly categoryLabel: string;
  readonly publishedAt: IsoDateTime;
  readonly cover: MediaDto | null;
  readonly tags: readonly TermDto[];
  readonly isFeatured: boolean;
  readonly path: string;
}

export interface NewsDetailDto {
  readonly slug: string;
  readonly title: string;
  readonly summary: string;
  readonly bodyHtml: string;
  readonly category: NewsCategory;
  readonly categoryLabel: string;
  readonly publishedAt: IsoDateTime;
  readonly updatedAt: IsoDateTime;
  readonly cover: MediaDto | null;
  readonly gallery: readonly MediaDto[];
  readonly tags: readonly TermDto[];
  readonly relatedEvent: EventCardDto | null;
  readonly related: readonly NewsCardDto[];
  readonly seo: SeoDto;
  readonly path: string;
}

export interface CategoryCountDto {
  readonly value: NewsCategory;
  readonly label: string;
  readonly count: number;
}

export interface NewsQuery {
  readonly category?: NewsCategory;
  readonly tag?: string;
  readonly year?: number;
  readonly q?: string;
  readonly page?: number;
  readonly pageSize?: number;
}
