import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { EventsApi } from '../../../core/api/events.api';
import { HomeApi } from '../../../core/api/home.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { HomeDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { SiteStore } from '../../../core/site/site.store';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { HomeAbout } from '../home-about/home-about';
import { HomeAmbassadors } from '../home-ambassadors/home-ambassadors';
import { HomeArchive } from '../home-archive/home-archive';
import { HomeFeatured } from '../home-featured/home-featured';
import { HomeHero } from '../home-hero/home-hero';
import { HomeNews } from '../home-news/home-news';
import { HomePartners } from '../home-partners/home-partners';
import { HomePeople } from '../home-people/home-people';
import { HomeUpcoming } from '../home-upcoming/home-upcoming';

type NumberedSection = 'about' | 'upcoming' | 'news' | 'archive' | 'people' | 'ambassadors' | 'partners';

/** Project home page: loads the home blocks and lays out the sections, numbering only visible ones. */
@Component({
  selector: 'sd-home-page',
  imports: [
    ErrorState,
    LoadingState,
    HomeAbout,
    HomeAmbassadors,
    HomeArchive,
    HomeFeatured,
    HomeHero,
    HomeNews,
    HomePartners,
    HomePeople,
    HomeUpcoming,
  ],
  templateUrl: './home-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePage {
  private readonly homeApi = inject(HomeApi);
  private readonly eventsApi = inject(EventsApi);
  private readonly seo = inject(SeoService);
  private readonly language = inject(LanguageService).language;
  protected readonly project = inject(ProjectContext);
  protected readonly site = inject(SiteStore);

  protected readonly home = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.language() }),
    stream: ({ params }) => this.homeApi.getHome(params.slug),
  });

  /** Disciplines for the "fields" card come from the event filter facets. */
  private readonly filters = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.language() }),
    stream: ({ params }) => this.eventsApi.filters(params.slug),
  });

  protected readonly disciplines = computed(() => (this.filters.hasValue() ? this.filters.value().disciplines : []));
  protected readonly data = computed<HomeDto | null>(() => (this.home.hasValue() ? this.home.value() : null));

  /** "01", "02", … assigned in page order to the sections that actually render. */
  protected readonly sectionIndex = computed<Readonly<Partial<Record<NumberedSection, string>>>>(() => {
    const home = this.data();
    if (!home) {
      return {};
    }
    const visible: readonly [NumberedSection, boolean][] = [
      ['about', !!home.about],
      ['upcoming', true],
      ['news', home.latestNews.length > 0],
      ['archive', !home.archive.isEmpty],
      ['people', home.people.length > 0],
      ['ambassadors', !!home.ambassadors],
      ['partners', home.funding.funders.length + home.funding.partners.length > 0],
    ];
    const result: Partial<Record<NumberedSection, string>> = {};
    let counter = 0;
    for (const [key, shown] of visible) {
      if (shown) {
        counter += 1;
        result[key] = counter.toString().padStart(2, '0');
      }
    }
    return result;
  });

  constructor() {
    effect(() => {
      const home = this.data();
      if (home) {
        this.seo.apply(home.seo);
      }
    });
  }
}
