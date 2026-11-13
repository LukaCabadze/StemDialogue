import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { PagesApi } from '../../../core/api/pages.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { AboutDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { ActionLink } from '../../../shared/ui/action-link/action-link';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';
import { PartnerGroups } from '../../../shared/ui/partner-groups/partner-groups';
import { RichText } from '../../../shared/ui/rich-text/rich-text';
import { MilestoneTimeline } from '../milestone-timeline/milestone-timeline';

/** About the project: CMS body, milestone timeline and the funding / partnership statement (#funding). */
@Component({
  selector: 'sd-about-page',
  imports: [ActionLink, ErrorState, LoadingState, MilestoneTimeline, PageHero, PartnerGroups, RichText],
  templateUrl: './about-page.html',
  styleUrl: './about-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AboutPage {
  private readonly api = inject(PagesApi);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  protected readonly about = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.getAbout(params.slug),
  });

  protected readonly data = computed<AboutDto | null>(() => (this.about.hasValue() ? this.about.value() : null));
  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.data()?.page.title ?? this.text().nav.about },
  ]);

  constructor() {
    effect(() => {
      const about = this.data();
      if (about) {
        this.seo.apply(about.page.seo);
      }
    });
  }
}
