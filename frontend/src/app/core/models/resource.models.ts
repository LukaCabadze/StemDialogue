import { IsoDateTime, MediaDto, SeoDto, TermDto } from './common.models';
import { ResourceType } from './enums';
import { PersonRefDto } from './person.models';

export interface EventRefDto {
  readonly slug: string;
  readonly title: string;
  readonly startsAt: IsoDateTime;
  readonly path: string;
}

export interface ResourceCardDto {
  readonly slug: string;
  readonly title: string;
  readonly description: string | null;
  readonly type: ResourceType;
  readonly typeLabel: string;
  readonly url: string;
  readonly embedUrl: string | null;
  readonly file: MediaDto | null;
  readonly thumbnail: MediaDto | null;
  readonly authorName: string | null;
  readonly author: PersonRefDto | null;
  readonly event: EventRefDto | null;
  readonly publishedAt: IsoDateTime | null;
  readonly tags: readonly TermDto[];
  readonly path: string;
}

export interface ResourceDetailDto {
  readonly resource: ResourceCardDto;
  readonly disciplines: readonly TermDto[];
  readonly related: readonly ResourceCardDto[];
  readonly seo: SeoDto;
}

export interface ResourceQuery {
  readonly type?: ResourceType;
  readonly event?: string;
  readonly discipline?: string;
  readonly tag?: string;
  readonly q?: string;
  readonly page?: number;
  readonly pageSize?: number;
}
