import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { AmbassadorsTeaserDto } from '../../../core/models';
import { Icon } from '../../../shared/ui/icon/icon';
import { NumberedSteps } from '../../../shared/ui/numbered-steps/numbered-steps';

/** "06 · STEM Ambassadors": programme teaser with the three-step path on a pink panel. */
@Component({
  selector: 'sd-home-ambassadors',
  imports: [RouterLink, Icon, NumberedSteps],
  templateUrl: './home-ambassadors.html',
  styleUrl: './home-ambassadors.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeAmbassadors {
  protected readonly text = inject(LanguageService).text;

  readonly teaser = input.required<AmbassadorsTeaserDto>();
  readonly index = input('06');
}
