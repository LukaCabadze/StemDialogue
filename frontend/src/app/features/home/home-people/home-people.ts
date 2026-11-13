import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { PeopleEntryDto } from '../../../core/models';
import { Icon } from '../../../shared/ui/icon/icon';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';

const MAX_FACES = 4;

/** "05 · People": entry cards for the team, researchers and mentors with stacked portraits. */
@Component({
  selector: 'sd-home-people',
  imports: [RouterLink, Icon, SectionHeader],
  templateUrl: './home-people.html',
  styleUrl: './home-people.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePeople {
  protected readonly text = inject(LanguageService).text;

  readonly entries = input.required<readonly PeopleEntryDto[]>();
  readonly index = input('05');

  /** Placeholder circles when no portraits are uploaded yet. */
  protected placeholders(entry: PeopleEntryDto): readonly number[] {
    const count = Math.min(MAX_FACES, Math.max(entry.count, 1));
    return entry.photos.length > 0 ? [] : Array.from({ length: count }, (_, index) => index);
  }

  protected photos(entry: PeopleEntryDto) {
    return entry.photos.slice(0, MAX_FACES);
  }
}
