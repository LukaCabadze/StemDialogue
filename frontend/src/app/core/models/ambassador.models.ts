import { IsoDate, MediaDto, SeoDto } from './common.models';
import { PageDto } from './page.models';

export interface SchoolActivityCardDto {
  readonly slug: string;
  readonly title: string;
  readonly summary: string | null;
  readonly schoolName: string | null;
  readonly region: string | null;
  readonly activityDate: IsoDate | null;
  readonly cover: MediaDto | null;
  readonly path: string;
}

export interface SchoolActivityDetailDto {
  readonly activity: SchoolActivityCardDto;
  readonly bodyHtml: string | null;
  readonly gallery: readonly MediaDto[];
  readonly seo: SeoDto;
}

/** Program information page plus the school-activity archive. */
export interface AmbassadorsDto {
  readonly page: PageDto | null;
  readonly latestActivities: readonly SchoolActivityCardDto[];
  readonly activityCount: number;
}
