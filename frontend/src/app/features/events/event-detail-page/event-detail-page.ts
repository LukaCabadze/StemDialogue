import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { EventsApi } from '../../../core/api/events.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventDetailDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { GalleryGrid } from '../../../shared/ui/gallery-grid/gallery-grid';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { ResourceItem } from '../../../shared/ui/resource-item/resource-item';
import { RichText } from '../../../shared/ui/rich-text/rich-text';
import { EventDetailHeader } from '../event-detail-header/event-detail-header';
import { EventRecording } from '../event-recording/event-recording';
import { EventSidebar } from '../event-sidebar/event-sidebar';
import { EventStatusNotice } from '../event-status-notice/event-status-notice';
import { SpeakerCard } from '../speaker-card/speaker-card';

/**
 * One page for the whole event lifecycle: announcement and registration before the meeting,
 * recording, summary, gallery and materials after it.
 */
@Component({
  selector: 'sd-event-detail-page',
  imports: [
    EmptyState,
    ErrorState,
    EventDetailHeader,
    EventRecording,
    EventSidebar,
    EventStatusNotice,
    GalleryGrid,
    LoadingState,
    ResourceItem,
    RichText,
    SpeakerCard,
  ],
  templateUrl: './event-detail-page.html',
  styleUrl: './event-detail-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventDetailPage {
  private readonly api = inject(EventsApi);
  private readonly seo = inject(SeoService);
  private readonly project = inject(ProjectContext);
  private readonly languages = inject(LanguageService);
  protected readonly text = this.languages.text;

  /** Route parameter `:slug`. */
  readonly slug = input.required<string>();

  protected readonly event = rxResource({
    params: () => ({ project: this.project.slug(), slug: this.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.get(params.project, params.slug),
  });

  protected readonly data = computed<EventDetailDto | null>(() => (this.event.hasValue() ? this.event.value() : null));
  protected readonly isCompleted = computed(() => this.data()?.status === 'Completed');
  protected readonly showNotice = computed(() => {
    const status = this.data()?.status;
    return status === 'Postponed' || status === 'Cancelled';
  });

  constructor() {
    effect(() => {
      const event = this.data();
      if (event) {
        this.seo.apply(event.seo);
      }
    });
  }
}
