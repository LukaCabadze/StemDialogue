import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { FundingDto } from '../../../core/models';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';
import { PartnerJourney } from '../partner-journey/partner-journey';

/** "07 · Partners": the funder and partner universities as a scroll-drawn journey. */
@Component({
  selector: 'sd-home-partners',
  imports: [PartnerJourney, SectionHeader],
  templateUrl: './home-partners.html',
  styleUrl: './home-partners.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomePartners {
  protected readonly text = inject(LanguageService).text;

  readonly funding = input.required<FundingDto>();
  readonly index = input('07');
}
