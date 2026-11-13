import { IsoDate, MediaDto, SeoDto, TermDto } from './common.models';
import { PersonLinkType, PersonRole } from './enums';
import { EventCardDto } from './event.models';
import { PageDto } from './page.models';
import { ResourceCardDto } from './resource.models';

/** Compact person reference used on event and resource cards. */
export interface PersonRefDto {
  readonly slug: string;
  readonly fullName: string;
  readonly academicTitle: string | null;
  readonly organization: string | null;
  readonly photo: MediaDto | null;
  readonly role: string | null;
  readonly path: string;
}

export interface PersonCardDto {
  readonly slug: string;
  readonly fullName: string;
  readonly academicTitle: string | null;
  readonly position: string | null;
  readonly organization: string | null;
  readonly shortBio: string | null;
  readonly photo: MediaDto | null;
  readonly roles: readonly PersonRole[];
  readonly disciplines: readonly TermDto[];
  readonly isCurrent: boolean;
  readonly path: string;
}

export interface TeamMemberDto {
  readonly slug: string;
  readonly fullName: string;
  readonly roleTitle: string | null;
  readonly academicTitle: string | null;
  readonly organization: string | null;
  readonly shortBio: string | null;
  readonly photo: MediaDto | null;
  readonly since: IsoDate | null;
  readonly until: IsoDate | null;
  readonly path: string;
}

export interface TeamSectionDto {
  readonly role: PersonRole;
  readonly title: string;
  readonly isOfficialGrantStaff: boolean;
  readonly current: readonly TeamMemberDto[];
  readonly former: readonly TeamMemberDto[];
}

export interface TeamDto {
  readonly page: PageDto | null;
  readonly sections: readonly TeamSectionDto[];
}

export interface PersonRoleDto {
  readonly role: PersonRole;
  readonly roleLabel: string;
  readonly title: string | null;
  readonly since: IsoDate | null;
  readonly until: IsoDate | null;
  readonly isCurrent: boolean;
}

export interface PersonLinkDto {
  readonly type: PersonLinkType;
  readonly url: string;
  readonly label: string | null;
}

export interface PersonDetailDto {
  readonly slug: string;
  readonly fullName: string;
  readonly academicTitle: string | null;
  readonly position: string | null;
  readonly organization: string | null;
  readonly shortBio: string | null;
  readonly biographyHtml: string | null;
  readonly contribution: string | null;
  readonly photo: MediaDto | null;
  readonly roles: readonly PersonRoleDto[];
  readonly links: readonly PersonLinkDto[];
  readonly disciplines: readonly TermDto[];
  readonly upcomingEvents: readonly EventCardDto[];
  readonly pastEvents: readonly EventCardDto[];
  readonly resources: readonly ResourceCardDto[];
  readonly seo: SeoDto;
  readonly path: string;
}

export interface PeopleQuery {
  readonly q?: string;
  readonly discipline?: string;
  readonly includeFormer?: boolean;
  readonly page?: number;
  readonly pageSize?: number;
}
