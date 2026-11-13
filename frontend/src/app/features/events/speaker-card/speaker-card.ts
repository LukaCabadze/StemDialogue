import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { PersonRefDto } from '../../../core/models';
import { Avatar } from '../../../shared/ui/avatar/avatar';
import { Icon } from '../../../shared/ui/icon/icon';

/** Speaker / mentor on an event page: 72px portrait, role, name, affiliation, profile link. */
@Component({
  selector: 'sd-speaker-card',
  imports: [RouterLink, Avatar, Icon],
  templateUrl: './speaker-card.html',
  styleUrl: './speaker-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeakerCard {
  protected readonly text = inject(LanguageService).text;

  readonly person = input.required<PersonRefDto>();

  protected readonly affiliation = computed(() =>
    [this.person().academicTitle, this.person().organization].filter((part) => !!part).join(' · '),
  );
}
