import { Injectable, computed, inject, signal } from '@angular/core';

import { AdminApi } from './admin-api.service';
import { LabeledValue, Lookups } from './admin.models';

const EMPTY: Lookups = {
  newsCategories: [],
  eventTypes: [],
  eventFormats: [],
  eventStatuses: [],
  participantRoles: [],
  personRoles: [],
  personLinkTypes: [],
  resourceTypes: [],
  partnerRoles: [],
  socialPlatforms: [],
  publicationStatuses: [],
  pageKeys: [],
  allowedUploadExtensions: [],
  people: [],
  events: [],
  tags: [],
  disciplines: [],
  organizations: [],
  projects: [],
};

/** Enum labels and entity lists for form selects. Reloaded after edits that change them. */
@Injectable()
export class AdminLookups {
  private readonly api = inject(AdminApi);
  readonly value = signal<Lookups>(EMPTY);
  readonly loaded = signal(false);

  private readonly labels = computed(() => {
    const map = new Map<string, string>();
    const v = this.value();
    for (const list of [
      v.newsCategories,
      v.eventTypes,
      v.eventFormats,
      v.eventStatuses,
      v.participantRoles,
      v.personRoles,
      v.personLinkTypes,
      v.resourceTypes,
      v.partnerRoles,
      v.publicationStatuses,
    ] as LabeledValue[][]) {
      for (const item of list) {
        map.set(item.value, item.label);
      }
    }
    return map;
  });

  reload(): void {
    this.api.lookups().subscribe({
      next: (lookups) => {
        this.value.set(lookups);
        this.loaded.set(true);
      },
      error: () => this.loaded.set(true),
    });
  }

  /** Georgian label of an enum value (falls back to the value). */
  label(value: string | null | undefined): string {
    return value ? (this.labels().get(value) ?? value) : '—';
  }
}
