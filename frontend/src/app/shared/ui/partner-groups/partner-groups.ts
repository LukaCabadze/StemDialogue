import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { FundingDto } from '../../../core/models';
import { PartnerCard } from '../partner-card/partner-card';

/** Funder, partner universities and supporters as separately labelled logo groups. */
@Component({
  selector: 'sd-partner-groups',
  imports: [PartnerCard],
  templateUrl: './partner-groups.html',
  styleUrl: './partner-groups.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PartnerGroups {
  protected readonly text = inject(LanguageService).text;

  readonly funding = input.required<FundingDto>();
}
