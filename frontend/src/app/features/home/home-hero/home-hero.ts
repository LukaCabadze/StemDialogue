import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { DateFormatService } from '../../../core/i18n/date-format.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventCardDto, PageDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { ActionLink } from '../../../shared/ui/action-link/action-link';
import { AtomMark } from '../../../shared/ui/atom-mark/atom-mark';
import { DateChip } from '../../../shared/ui/date-chip/date-chip';

/** Home hero: project pill, title + tagline, lead, two actions, brand orbit and next-meeting card. */
@Component({
  selector: 'sd-home-hero',
  imports: [RouterLink, ActionLink, AtomMark, DateChip],
  templateUrl: './home-hero.html',
  styleUrl: './home-hero.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeHero {
  private readonly dates = inject(DateFormatService);
  private readonly project = inject(ProjectContext);
  protected readonly text = inject(LanguageService).text;

  readonly hero = input<PageDto | null>(null);
  readonly tagline = input<string | null>(null);
  readonly projectYears = input('');
  readonly nextEvent = input<EventCardDto | null>(null);

  protected readonly title = computed(() => this.hero()?.title ?? this.text().brand.name);
  protected readonly subtitle = computed(() => this.tagline() ?? this.text().brand.tagline);
  protected readonly primary = computed(
    () => this.hero()?.primaryAction ?? { label: this.text().nav.stemSaturdays, url: this.project.path('stem-saturdays') },
  );
  protected readonly secondary = computed(
    () => this.hero()?.secondaryAction ?? { label: this.text().nav.about, url: this.project.path('about') },
  );
  protected readonly nextEventTime = computed(() => this.dates.format(this.nextEvent()?.startsAt, 'time'));
  protected readonly registrationOpen = computed(() => !!this.nextEvent()?.registrationUrl);
}
