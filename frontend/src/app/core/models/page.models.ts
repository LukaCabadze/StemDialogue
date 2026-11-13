import { IsoDate, MediaDto, SeoDto } from './common.models';
import { MilestoneState } from './enums';
import { ContactDto, FundingDto, SocialLinkDto } from './site.models';

export interface ActionDto {
  readonly label: string;
  readonly url: string;
}

export interface PageDto {
  readonly key: string;
  readonly title: string;
  readonly lead: string | null;
  readonly bodyHtml: string | null;
  readonly heroImage: MediaDto | null;
  readonly primaryAction: ActionDto | null;
  readonly secondaryAction: ActionDto | null;
  readonly seo: SeoDto;
  readonly path: string;
}

export interface MilestoneDto {
  readonly id: string;
  readonly title: string;
  readonly description: string | null;
  readonly periodLabel: string | null;
  readonly location: string | null;
  readonly startDate: IsoDate | null;
  readonly endDate: IsoDate | null;
  readonly state: MilestoneState;
}

export interface AboutDto {
  readonly page: PageDto;
  readonly milestones: readonly MilestoneDto[];
  readonly funding: FundingDto;
}

export interface ContactPageDto {
  readonly page: PageDto;
  readonly contact: ContactDto;
  readonly socialLinks: readonly SocialLinkDto[];
  readonly funding: FundingDto;
}
