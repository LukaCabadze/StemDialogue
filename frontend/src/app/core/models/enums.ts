// String unions mirroring the API enums (serialised by name).

export type Language = 'ka' | 'en';

export type ProjectStatus = 'Upcoming' | 'Active' | 'Completed';
export type PartnerRole = 'Funder' | 'PartnerUniversity' | 'Supporter';
export type SocialPlatform = 'Facebook' | 'Instagram' | 'YouTube' | 'LinkedIn' | 'X' | 'TikTok' | 'Website' | 'Other';
export type MediaKind = 'Image' | 'Document' | 'Presentation' | 'Spreadsheet';

export type EventType = 'StemSaturday' | 'Workshop' | 'Lecture' | 'Forum' | 'FinalEvent' | 'Other';
export type EventFormat = 'Online' | 'InPerson' | 'Hybrid';
export type EventStatus = 'Planned' | 'Postponed' | 'Completed' | 'Cancelled';
export type EventScope = 'Upcoming' | 'Past' | 'All';

export type NewsCategory =
  | 'Announcement'
  | 'EventAnnouncement'
  | 'EventSummary'
  | 'Opportunity'
  | 'Achievement'
  | 'General';

export type PersonRole = 'CoreTeam' | 'Consultant' | 'TechnologyTeam' | 'OrganizationalStaff' | 'Researcher' | 'Mentor';
export type PersonLinkType = 'Website' | 'University' | 'Orcid' | 'GoogleScholar' | 'ResearchGate' | 'LinkedIn' | 'Other';

export type ResourceType = 'Document' | 'Presentation' | 'Video' | 'Photo' | 'Link' | 'Other';

export type MilestoneState = 'Upcoming' | 'Current' | 'Done';
