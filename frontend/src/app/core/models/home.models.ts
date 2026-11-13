import { MediaDto, SeoDto } from './common.models';
import { EventCardDto } from './event.models';
import { NewsCardDto } from './news.models';
import { MilestoneDto, PageDto } from './page.models';
import { ResourceCardDto } from './resource.models';
import { FundingDto } from './site.models';

export interface AboutTeaserDto {
  readonly title: string;
  readonly lead: string | null;
  readonly path: string;
  readonly milestones: readonly MilestoneDto[];
}

/** Current highlight: a featured announcement or the next meeting. */
export interface FeaturedDto {
  readonly kind: string;
  readonly news: NewsCardDto | null;
  readonly event: EventCardDto | null;
}

export interface ArchiveHighlightsDto {
  readonly recentEvents: readonly EventCardDto[];
  readonly resources: readonly ResourceCardDto[];
  readonly isEmpty: boolean;
}

export interface PeopleEntryDto {
  readonly key: string;
  readonly title: string;
  readonly lead: string | null;
  readonly count: number;
  readonly photos: readonly MediaDto[];
  readonly path: string;
}

export interface AmbassadorsTeaserDto {
  readonly title: string;
  readonly lead: string | null;
  readonly activityCount: number;
  readonly path: string;
}

/** Home blocks; empty blocks arrive as null / empty lists so the UI can hide them. */
export interface HomeDto {
  readonly hero: PageDto | null;
  readonly about: AboutTeaserDto | null;
  readonly featured: FeaturedDto | null;
  readonly upcomingEvents: readonly EventCardDto[];
  readonly latestNews: readonly NewsCardDto[];
  readonly archive: ArchiveHighlightsDto;
  readonly people: readonly PeopleEntryDto[];
  readonly ambassadors: AmbassadorsTeaserDto | null;
  readonly funding: FundingDto;
  readonly seo: SeoDto;
}
