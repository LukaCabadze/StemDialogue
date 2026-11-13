import { Injectable, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { SiteApi } from '../api/site.api';
import { LanguageService } from '../i18n/language.service';
import { FundingDto, PartnerDto, SiteDto } from '../models';
import { ProjectContext } from '../project/project-context.service';

/** Project-wide layout data, loaded once per project and language and shared by header and footer. */
@Injectable({ providedIn: 'root' })
export class SiteStore {
  private readonly api = inject(SiteApi);
  private readonly project = inject(ProjectContext);
  private readonly language = inject(LanguageService);

  private readonly resource = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.language.language() }),
    stream: ({ params }) => this.api.getSite(params.slug),
  });

  /** Last successfully loaded site (kept while a language switch reloads it). */
  readonly site = computed<SiteDto | null>(() => (this.resource.hasValue() ? this.resource.value() : null));
  readonly isLoading = this.resource.isLoading;
  readonly funding = computed<FundingDto | null>(() => this.site()?.funding ?? null);
  readonly funders = computed<readonly PartnerDto[]>(() => this.funding()?.funders ?? []);
  readonly partners = computed<readonly PartnerDto[]>(() => this.funding()?.partners ?? []);
  readonly contact = computed(() => this.site()?.contact ?? null);
  readonly socialLinks = computed(() => this.site()?.socialLinks ?? []);
  readonly navigation = computed(() => this.site()?.navigation ?? []);
  /** "2026–2027" for the hero pill and footer. */
  readonly projectYears = computed(() => {
    const site = this.site();
    if (!site) {
      return '';
    }
    const start = site.startDate.slice(0, 4);
    const end = site.endDate.slice(0, 4);
    return start === end ? start : `${start}–${end}`;
  });

  reload(): void {
    this.resource.reload();
  }
}
