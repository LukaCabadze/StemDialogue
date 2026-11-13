import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { AboutTeaserDto, TermDto } from '../../../core/models';
import { Icon } from '../../../shared/ui/icon/icon';
import { IconName } from '../../../shared/ui/icon/icon-paths';
import { Pill } from '../../../shared/ui/pill/pill';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';

/** "01 · About": goal / audience / fields cards and the four-step milestone strip. */
@Component({
  selector: 'sd-home-about',
  imports: [Icon, Pill, SectionHeader],
  templateUrl: './home-about.html',
  styleUrl: './home-about.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeAbout {
  protected readonly text = inject(LanguageService).text;

  readonly about = input.required<AboutTeaserDto>();
  readonly disciplines = input<readonly TermDto[]>([]);
  readonly index = input('01');

  protected readonly icons: Readonly<Record<'goal' | 'audience' | 'directions', IconName>> = {
    goal: 'target',
    audience: 'users',
    directions: 'compass',
  };
}
