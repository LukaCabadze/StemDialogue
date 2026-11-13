import { IsoDate, MediaDto, SeoDto } from './common.models';
import { Language, PartnerRole, ProjectStatus, SocialPlatform } from './enums';

export interface PartnerDto {
  readonly organizationId: string;
  readonly role: PartnerRole;
  readonly roleLabel: string;
  readonly name: string;
  readonly shortName: string;
  readonly description: string | null;
  readonly websiteUrl: string | null;
  readonly logo: MediaDto | null;
  readonly logoOnDark: MediaDto | null;
  readonly showInHeader: boolean;
}

/** Funder and partner universities are separate lists so they are never shown in the same role. */
export interface FundingDto {
  readonly shortText: string;
  readonly fullText: string;
  readonly funders: readonly PartnerDto[];
  readonly partners: readonly PartnerDto[];
  readonly supporters: readonly PartnerDto[];
}

export interface ContactDto {
  readonly person: string | null;
  readonly email: string | null;
  readonly phone: string | null;
  readonly address: string | null;
}

export interface SocialLinkDto {
  readonly platform: SocialPlatform;
  readonly url: string;
}

export interface NavItemDto {
  readonly key: string;
  readonly label: string;
  readonly path: string;
}

export interface SiteDto {
  readonly slug: string;
  readonly code: string;
  readonly name: string;
  readonly shortName: string;
  readonly tagline: string | null;
  readonly status: ProjectStatus;
  readonly startDate: IsoDate;
  readonly endDate: IsoDate;
  readonly logo: MediaDto | null;
  readonly funding: FundingDto;
  readonly contact: ContactDto;
  readonly socialLinks: readonly SocialLinkDto[];
  readonly navigation: readonly NavItemDto[];
  readonly seo: SeoDto;
  readonly language: Language;
  readonly availableLanguages: readonly Language[];
}
