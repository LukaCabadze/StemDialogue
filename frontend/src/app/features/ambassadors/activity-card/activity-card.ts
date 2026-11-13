import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { SchoolActivityCardDto } from '../../../core/models';
import { coverVariantFor } from '../../../core/util/cover-variant';
import { SdDatePipe } from '../../../shared/pipes/sd-date.pipe';
import { CoverArt } from '../../../shared/ui/cover-art/cover-art';
import { Icon } from '../../../shared/ui/icon/icon';

/** School initiative card: cover, school · region, date, title and summary. */
@Component({
  selector: 'sd-activity-card',
  imports: [RouterLink, SdDatePipe, CoverArt, Icon],
  templateUrl: './activity-card.html',
  styleUrl: './activity-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActivityCard {
  protected readonly language = inject(LanguageService).language;

  readonly activity = input.required<SchoolActivityCardDto>();

  protected readonly variant = computed(() => coverVariantFor(this.activity().slug, 1));
  protected readonly place = computed(() =>
    [this.activity().schoolName, this.activity().region].filter((part) => !!part).join(' · '),
  );
}
