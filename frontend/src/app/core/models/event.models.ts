import { IsoDateTime, LabeledValue, MediaDto, SeoDto, TermDto } from './common.models';
import { EventFormat, EventScope, EventStatus, EventType } from './enums';
import { NewsCardDto } from './news.models';
import { PersonRefDto } from './person.models';
import { ResourceCardDto } from './resource.models';

export interface EventCardDto {
  readonly slug: string;
  readonly title: string;
  readonly summary: string | null;
  readonly type: EventType;
  readonly typeLabel: string;
  readonly status: EventStatus;
  readonly statusLabel: string;
  readonly format: EventFormat;
  readonly formatLabel: string;
  readonly startsAt: IsoDateTime;
  readonly endsAt: IsoDateTime | null;
  readonly venue: string | null;
  readonly cover: MediaDto | null;
  readonly speakers: readonly PersonRefDto[];
  readonly disciplines: readonly TermDto[];
  readonly tags: readonly TermDto[];
  readonly isUpcoming: boolean;
  readonly hasRecording: boolean;
  readonly hasMaterials: boolean;
  readonly registrationUrl: string | null;
  readonly path: string;
}

/** One URL for the whole lifecycle: announcement first, archive fields once completed. */
export interface EventDetailDto {
  readonly slug: string;
  readonly title: string;
  readonly summary: string | null;
  readonly descriptionHtml: string;
  readonly type: EventType;
  readonly typeLabel: string;
  readonly status: EventStatus;
  readonly statusLabel: string;
  readonly statusNote: string | null;
  readonly format: EventFormat;
  readonly formatLabel: string;
  readonly startsAt: IsoDateTime;
  readonly endsAt: IsoDateTime | null;
  readonly originalStartsAt: IsoDateTime | null;
  readonly timeZone: string;
  readonly venue: string | null;
  readonly streamUrl: string | null;
  readonly registrationUrl: string | null;
  readonly registrationDeadline: IsoDateTime | null;
  readonly isRegistrationOpen: boolean;
  readonly cover: MediaDto | null;
  readonly participants: readonly PersonRefDto[];
  readonly disciplines: readonly TermDto[];
  readonly tags: readonly TermDto[];
  readonly recapHtml: string | null;
  readonly recordingUrl: string | null;
  readonly recordingEmbedUrl: string | null;
  readonly gallery: readonly MediaDto[];
  readonly resources: readonly ResourceCardDto[];
  readonly relatedNews: readonly NewsCardDto[];
  readonly seo: SeoDto;
  readonly path: string;
}

export interface EventFiltersDto {
  readonly years: readonly number[];
  readonly disciplines: readonly TermDto[];
  readonly tags: readonly TermDto[];
  readonly formats: readonly LabeledValue[];
  readonly types: readonly LabeledValue[];
}

export interface EventQuery {
  readonly scope?: EventScope;
  readonly year?: number;
  readonly month?: number;
  readonly discipline?: string;
  readonly tag?: string;
  readonly format?: EventFormat;
  readonly type?: EventType;
  readonly q?: string;
  readonly page?: number;
  readonly pageSize?: number;
}
