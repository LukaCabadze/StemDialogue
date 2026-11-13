import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';

import { ResourcesApi } from '../../../core/api/resources.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { ResourceDetailDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { isTrustedEmbedUrl } from '../../../core/util/external-url';
import { FileSizePipe } from '../../../shared/pipes/file-size.pipe';
import { SdDatePipe } from '../../../shared/pipes/sd-date.pipe';
import { Breadcrumbs } from '../../../shared/ui/breadcrumbs/breadcrumbs';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { Icon } from '../../../shared/ui/icon/icon';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { Pill } from '../../../shared/ui/pill/pill';
import { ResourceItem } from '../../../shared/ui/resource-item/resource-item';
import { fileExtension, resourceIcon } from '../../../shared/ui/resource-item/resource-icon';
import { ShareButtons } from '../../../shared/ui/share-buttons/share-buttons';

/** One material: preview (embedded video or image), download / open action, source meeting and author. */
@Component({
  selector: 'sd-resource-detail-page',
  imports: [
    RouterLink,
    Breadcrumbs,
    ErrorState,
    FileSizePipe,
    Icon,
    LoadingState,
    Pill,
    ResourceItem,
    SdDatePipe,
    ShareButtons,
  ],
  templateUrl: './resource-detail-page.html',
  styleUrl: './resource-detail-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceDetailPage {
  private readonly api = inject(ResourcesApi);
  private readonly seo = inject(SeoService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;
  protected readonly language = this.languages.language;

  /** Route parameter `:slug`. */
  readonly slug = input.required<string>();

  protected readonly detail = rxResource({
    params: () => ({ project: this.project.slug(), slug: this.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.get(params.project, params.slug),
  });

  protected readonly data = computed<ResourceDetailDto | null>(() => (this.detail.hasValue() ? this.detail.value() : null));
  protected readonly resource = computed(() => this.data()?.resource ?? null);
  protected readonly icon = computed(() => resourceIcon(this.resource()?.type ?? 'Other'));
  protected readonly extension = computed(() => fileExtension(this.resource()?.file?.contentType ?? this.resource()?.file?.url));

  // Only allow-listed https players are embedded.
  protected readonly embed = computed<SafeResourceUrl | null>(() => {
    const url = this.resource()?.embedUrl;
    return isTrustedEmbedUrl(url) ? this.sanitizer.bypassSecurityTrustResourceUrl(url) : null;
  });

  protected readonly previewImage = computed(() => {
    const resource = this.resource();
    if (!resource) {
      return null;
    }
    if (resource.type === 'Photo' && resource.file?.kind === 'Image') {
      return resource.file;
    }
    return resource.thumbnail;
  });

  protected readonly actionLabel = computed(() => {
    const copy = this.text().resources;
    const resource = this.resource();
    if (resource?.file) {
      return copy.download;
    }
    return resource?.type === 'Video' ? copy.watch : copy.open;
  });

  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().resources.title, path: this.project.path('resources') },
    { label: this.resource()?.title ?? '' },
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
