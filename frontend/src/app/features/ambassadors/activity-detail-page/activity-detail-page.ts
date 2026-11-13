import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { AmbassadorsApi } from '../../../core/api/ambassadors.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { SchoolActivityDetailDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { coverVariantFor } from '../../../core/util/cover-variant';
import { SdDatePipe } from '../../../shared/pipes/sd-date.pipe';
import { Breadcrumbs } from '../../../shared/ui/breadcrumbs/breadcrumbs';
import { CoverArt } from '../../../shared/ui/cover-art/cover-art';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { GalleryGrid } from '../../../shared/ui/gallery-grid/gallery-grid';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { RichText } from '../../../shared/ui/rich-text/rich-text';
import { ShareButtons } from '../../../shared/ui/share-buttons/share-buttons';

/** One school initiative run by STEM Ambassadors. */
@Component({
  selector: 'sd-activity-detail-page',
  imports: [Breadcrumbs, CoverArt, ErrorState, GalleryGrid, LoadingState, RichText, SdDatePipe, ShareButtons],
  templateUrl: './activity-detail-page.html',
  styleUrl: './activity-detail-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActivityDetailPage {
  private readonly api = inject(AmbassadorsApi);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;
  protected readonly language = this.languages.language;

  /** Route parameter `:slug`. */
  readonly slug = input.required<string>();

  protected readonly detail = rxResource({
    params: () => ({ project: this.project.slug(), slug: this.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.activity(params.project, params.slug),
  });

  protected readonly data = computed<SchoolActivityDetailDto | null>(() =>
    this.detail.hasValue() ? this.detail.value() : null,
  );
  protected readonly variant = computed(() => coverVariantFor(this.slug(), 1));
  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().ambassadors.title, path: this.project.path('stem-ambassadors') },
    { label: this.data()?.activity.title ?? '' },
  ]);

  constructor() {
    effect(() => {
      const detail = this.data();
      if (detail) {
        this.seo.apply(detail.seo);
      }
    });
  }
}
