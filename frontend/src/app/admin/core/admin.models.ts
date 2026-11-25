// Contracts of the CMS API (`/api/v1/admin/...` and `/api/v1/auth/...`). Enums travel by name.

/** UTC timestamp as returned by the API, e.g. `2026-11-28T08:00:00Z`. */
export type UtcDateTime = string;
/** Calendar date `YYYY-MM-DD`. */
export type DateOnly = string;

export interface LText {
  ka: string | null;
  en: string | null;
}

export type PublicationStatus = 'Draft' | 'Published' | 'Archived';
export type MediaKind = 'Image' | 'Document' | 'Presentation' | 'Spreadsheet';
export type UserRole = 'Admin' | 'Editor';

export interface Paged<T> {
  readonly items: T[];
  readonly total: number;
  readonly page: number;
  readonly pageSize: number;
  readonly totalPages: number;
  readonly hasMore: boolean;
}

export interface AuditInfo {
  readonly createdAtUtc: UtcDateTime;
  readonly createdBy: string | null;
  readonly updatedAtUtc: UtcDateTime;
  readonly updatedBy: string | null;
  readonly concurrencyStamp: string;
}

export interface MediaRef {
  readonly id: string;
  readonly url: string;
  readonly originalFileName: string;
  readonly altKa: string | null;
  readonly width: number | null;
  readonly height: number | null;
  readonly kind: MediaKind;
}

export interface LabeledValue {
  readonly value: string;
  readonly label: string;
}

export interface LookupItem {
  readonly id: string;
  readonly label: string;
}

export interface Lookups {
  readonly newsCategories: LabeledValue[];
  readonly eventTypes: LabeledValue[];
  readonly eventFormats: LabeledValue[];
  readonly eventStatuses: LabeledValue[];
  readonly participantRoles: LabeledValue[];
  readonly personRoles: LabeledValue[];
  readonly personLinkTypes: LabeledValue[];
  readonly resourceTypes: LabeledValue[];
  readonly partnerRoles: LabeledValue[];
  readonly socialPlatforms: LabeledValue[];
  readonly publicationStatuses: LabeledValue[];
  readonly pageKeys: string[];
  readonly allowedUploadExtensions: string[];
  readonly people: LookupItem[];
  readonly events: LookupItem[];
  readonly tags: LookupItem[];
  readonly disciplines: LookupItem[];
  readonly organizations: LookupItem[];
  readonly projects: LookupItem[];
}

// ---- Auth ---------------------------------------------------------------------------------------

export interface AuthUser {
  readonly id: string;
  readonly email: string;
  readonly fullName: string;
  readonly roles: string[];
}

export interface AccessTokenResponse {
  readonly accessToken: string;
  readonly expiresAtUtc: UtcDateTime;
  readonly user: AuthUser;
}

// ---- Dashboard & audit --------------------------------------------------------------------------

export interface AuditEntry {
  readonly id: number;
  readonly occurredAtUtc: UtcDateTime;
  readonly userName: string | null;
  readonly entityType: string;
  readonly entityId: string;
  readonly action: string;
  readonly changesJson: string | null;
}

export interface Dashboard {
  readonly publishedNews: number;
  readonly draftNews: number;
  readonly scheduledNews: number;
  readonly upcomingEvents: number;
  readonly eventsAwaitingCompletion: number;
  readonly publishedPeople: number;
  readonly draftPeople: number;
  readonly publishedResources: number;
  readonly imagesWithoutAltText: number;
  readonly mediaBytes: number;
  readonly recentChanges: AuditEntry[];
}

// ---- Projects -----------------------------------------------------------------------------------

export type ProjectStatus = 'Upcoming' | 'Active' | 'Completed';

export interface ProjectListItem {
  readonly id: string;
  readonly slug: string;
  readonly code: string;
  readonly name: string;
  readonly status: ProjectStatus;
  readonly startDate: DateOnly;
  readonly endDate: DateOnly;
  readonly path: string;
}

export interface PartnerAdmin {
  readonly organizationId: string;
  readonly organizationName: string;
  readonly role: string;
  readonly roleLabel: LText;
  readonly showInHeader: boolean;
  readonly displayOrder: number;
}

export interface SocialLink {
  platform: string;
  url: string;
}

export interface ProjectAdmin {
  readonly id: string;
  readonly slug: string;
  readonly code: string;
  readonly name: LText;
  readonly shortName: LText;
  readonly tagline: LText;
  readonly status: ProjectStatus;
  readonly startDate: DateOnly;
  readonly endDate: DateOnly;
  readonly isDefault: boolean;
  readonly fundingShortText: LText;
  readonly fundingFullText: LText;
  readonly logo: MediaRef | null;
  readonly contactPerson: LText;
  readonly contactEmail: string | null;
  readonly contactPhone: string | null;
  readonly contactAddress: LText;
  readonly seoTitle: LText;
  readonly seoDescription: LText;
  readonly shareImage: MediaRef | null;
  readonly partners: PartnerAdmin[];
  readonly socialLinks: SocialLink[];
  readonly audit: AuditInfo;
}

// ---- Pages & milestones -------------------------------------------------------------------------

export interface PageListItem {
  readonly key: string;
  readonly title: string;
  readonly showInNavigation: boolean;
  readonly navigationOrder: number;
  readonly updatedAtUtc: UtcDateTime;
  readonly updatedBy: string | null;
  readonly publicPath: string;
}

export interface PageAdmin {
  readonly id: string;
  readonly key: string;
  readonly title: LText;
  readonly navigationLabel: LText;
  readonly lead: LText;
  readonly body: LText;
  readonly heroImage: MediaRef | null;
  readonly primaryActionLabel: LText;
  readonly primaryActionUrl: string | null;
  readonly secondaryActionLabel: LText;
  readonly secondaryActionUrl: string | null;
  readonly showInNavigation: boolean;
  readonly navigationOrder: number;
  readonly seoTitle: LText;
  readonly seoDescription: LText;
  readonly shareImage: MediaRef | null;
  readonly publicPath: string;
  readonly audit: AuditInfo;
}

export interface MilestoneAdmin {
  readonly id: string;
  readonly title: LText;
  readonly description: LText;
  readonly periodLabel: LText;
  readonly location: LText;
  readonly startDate: DateOnly | null;
  readonly endDate: DateOnly | null;
  readonly displayOrder: number;
  readonly isVisible: boolean;
  readonly audit: AuditInfo;
}

// ---- News ---------------------------------------------------------------------------------------

export interface NewsListItem {
  readonly id: string;
  readonly slug: string;
  readonly title: string;
  readonly category: string;
  readonly status: PublicationStatus;
  readonly isLive: boolean;
  readonly isScheduled: boolean;
  readonly publishAtUtc: UtcDateTime | null;
  readonly isFeatured: boolean;
  readonly updatedAtUtc: UtcDateTime;
  readonly updatedBy: string | null;
  readonly publicPath: string;
}

export interface NewsAdmin {
  readonly id: string;
  readonly slug: string;
  readonly title: LText;
  readonly summary: LText;
  readonly body: LText;
  readonly category: string;
  readonly status: PublicationStatus;
  readonly publishAtUtc: UtcDateTime | null;
  readonly firstPublishedAtUtc: UtcDateTime | null;
  readonly isFeatured: boolean;
  readonly coverImage: MediaRef | null;
  readonly gallery: MediaRef[];
  readonly tagIds: string[];
  readonly relatedEventId: string | null;
  readonly seoTitle: LText;
  readonly seoDescription: LText;
  readonly shareImage: MediaRef | null;
  readonly publicPath: string;
  readonly audit: AuditInfo;
}

// ---- Events -------------------------------------------------------------------------------------

export type EventStatus = 'Planned' | 'Postponed' | 'Completed' | 'Cancelled';

export interface EventListItem {
  readonly id: string;
  readonly slug: string;
  readonly title: string;
  readonly type: string;
  readonly status: EventStatus;
  readonly publication: PublicationStatus;
  readonly startsAtUtc: UtcDateTime;
  readonly isFeatured: boolean;
  readonly awaitingCompletion: boolean;
  readonly participantCount: number;
  readonly updatedAtUtc: UtcDateTime;
  readonly updatedBy: string | null;
  readonly publicPath: string;
}

export interface EventParticipantAdmin {
  readonly personId: string;
  readonly fullName: string;
  readonly role: string;
  readonly sortOrder: number;
}

export interface EventAdmin {
  readonly id: string;
  readonly slug: string;
  readonly type: string;
  readonly title: LText;
  readonly summary: LText;
  readonly description: LText;
  readonly startsAtUtc: UtcDateTime;
  readonly endsAtUtc: UtcDateTime | null;
  readonly originalStartsAtUtc: UtcDateTime | null;
  readonly format: string;
  readonly venue: LText;
  readonly streamUrl: string | null;
  readonly registrationUrl: string | null;
  readonly registrationDeadlineUtc: UtcDateTime | null;
  readonly status: EventStatus;
  readonly statusNote: LText;
  readonly publication: PublicationStatus;
  readonly firstPublishedAtUtc: UtcDateTime | null;
  readonly isFeatured: boolean;
  readonly coverImage: MediaRef | null;
  readonly participants: EventParticipantAdmin[];
  readonly tagIds: string[];
  readonly disciplineIds: string[];
  readonly recap: LText;
  readonly recordingUrl: string | null;
  readonly completedAtUtc: UtcDateTime | null;
  readonly gallery: MediaRef[];
  readonly seoTitle: LText;
  readonly seoDescription: LText;
  readonly shareImage: MediaRef | null;
  readonly publicPath: string;
  readonly audit: AuditInfo;
}

// ---- People -------------------------------------------------------------------------------------

export interface PersonListItem {
  readonly id: string;
  readonly slug: string;
  readonly fullName: string;
  readonly organization: string | null;
  readonly status: PublicationStatus;
  readonly roles: string[];
  readonly hasPhoto: boolean;
  readonly eventCount: number;
  readonly updatedAtUtc: UtcDateTime;
  readonly updatedBy: string | null;
}

export interface PersonRoleAdmin {
  readonly id: string;
  readonly projectId: string;
  readonly role: string;
  readonly title: LText;
  readonly startDate: DateOnly | null;
  readonly endDate: DateOnly | null;
  readonly displayOrder: number;
  readonly isCurrent: boolean;
}

export interface PersonLink {
  type: string;
  url: string;
  label: LText | null;
}

export interface PersonAdmin {
  readonly id: string;
  readonly slug: string;
  readonly fullName: LText;
  readonly academicTitle: LText;
  readonly position: LText;
  readonly organization: LText;
  readonly shortBio: LText;
  readonly biography: LText;
  readonly contribution: LText;
  readonly photo: MediaRef | null;
  readonly email: string | null;
  readonly status: PublicationStatus;
  readonly displayOrder: number;
  readonly roles: PersonRoleAdmin[];
  readonly links: PersonLink[];
  readonly disciplineIds: string[];
  readonly seoTitle: LText;
  readonly seoDescription: LText;
  readonly shareImage: MediaRef | null;
  readonly publicPaths: string[];
  readonly audit: AuditInfo;
}

// ---- Resources & school activities --------------------------------------------------------------

export interface ResourceListItem {
  readonly id: string;
  readonly slug: string;
  readonly title: string;
  readonly type: string;
  readonly status: PublicationStatus;
  readonly isFeatured: boolean;
  readonly eventTitle: string | null;
  readonly publishedAtUtc: UtcDateTime | null;
  readonly updatedAtUtc: UtcDateTime;
  readonly updatedBy: string | null;
}

export interface ResourceAdmin {
  readonly id: string;
  readonly slug: string;
  readonly title: LText;
  readonly description: LText;
  readonly type: string;
  readonly file: MediaRef | null;
  readonly externalUrl: string | null;
  readonly authorId: string | null;
  readonly authorName: LText;
  readonly eventId: string | null;
  readonly thumbnail: MediaRef | null;
  readonly status: PublicationStatus;
  readonly publishedAtUtc: UtcDateTime | null;
  readonly isFeatured: boolean;
  readonly tagIds: string[];
  readonly disciplineIds: string[];
  readonly publicPath: string;
  readonly audit: AuditInfo;
}

export interface ActivityListItem {
  readonly id: string;
  readonly slug: string;
  readonly title: string;
  readonly schoolName: string | null;
  readonly activityDate: DateOnly | null;
  readonly status: PublicationStatus;
  readonly mediaConsentConfirmed: boolean;
  readonly updatedAtUtc: UtcDateTime;
}

export interface ActivityAdmin {
  readonly id: string;
  readonly slug: string;
  readonly title: LText;
  readonly summary: LText;
  readonly body: LText;
  readonly schoolName: LText;
  readonly region: LText;
  readonly activityDate: DateOnly | null;
  readonly coverImage: MediaRef | null;
  readonly gallery: MediaRef[];
  readonly status: PublicationStatus;
  readonly mediaConsentConfirmed: boolean;
  readonly consentReference: string | null;
  readonly publicPath: string;
  readonly audit: AuditInfo;
}

// ---- Library ------------------------------------------------------------------------------------

export interface OrganizationAdmin {
  readonly id: string;
  readonly name: LText;
  readonly shortName: LText;
  readonly description: LText;
  readonly websiteUrl: string | null;
  readonly logo: MediaRef | null;
  readonly logoOnDark: MediaRef | null;
  readonly usedByProjects: string[];
  readonly audit: AuditInfo;
}

export interface TermAdmin {
  readonly id: string;
  readonly slug: string;
  readonly name: LText;
  readonly displayOrder: number;
  readonly usageCount: number;
}

export interface MediaAdmin {
  readonly id: string;
  readonly url: string;
  readonly originalFileName: string;
  readonly contentType: string;
  readonly sizeBytes: number;
  readonly kind: MediaKind;
  readonly width: number | null;
  readonly height: number | null;
  readonly altText: LText;
  readonly caption: LText;
  readonly credit: LText;
  readonly sha256: string;
  readonly missingAltText: boolean;
  readonly audit: AuditInfo;
}

export interface MediaUsage {
  readonly contentType: string;
  readonly id: string;
  readonly title: string;
}

export interface UserAdmin {
  readonly id: string;
  readonly email: string;
  readonly fullName: string;
  readonly role: UserRole;
  readonly isActive: boolean;
  readonly isLockedOut: boolean;
  readonly createdAtUtc: UtcDateTime;
  readonly lastLoginAtUtc: UtcDateTime | null;
}

export function toMediaRef(media: MediaAdmin): MediaRef {
  return {
    id: media.id,
    url: media.url,
    originalFileName: media.originalFileName,
    altKa: media.altText.ka,
    width: media.width,
    height: media.height,
    kind: media.kind,
  };
}

export function lt(value?: LText | null): LText {
  return { ka: value?.ka ?? '', en: value?.en ?? '' };
}
