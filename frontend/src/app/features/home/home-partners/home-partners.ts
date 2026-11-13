import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { FundingDto } from '../../../core/models';
import { PartnerGroups } from '../../../shared/ui/partner-groups/partner-groups';

/** "07 · Partners": funder and partner universities in clearly separated groups. */
@Component({
  selector: 'sd-home-partners',
  imports: [PartnerGroups],
  templateUrl: './home-partners.html',
  styleUrl: './home-partners.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePartners {
  protected readonly text = inject(LanguageService).text;

  readonly funding = input.required<FundingDto>();
  readonly index = input('07');
}
