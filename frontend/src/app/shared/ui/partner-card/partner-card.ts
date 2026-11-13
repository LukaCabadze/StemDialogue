import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { PartnerDto } from '../../../core/models';
import { displayDomain } from '../../../core/util/external-url';
import { partnerLogoSrc } from './partner-logos';

/** 216×116 logo tile with the domain and ↗ underneath; links to the partner's website. */
@Component({
  selector: 'sd-partner-card',
  imports: [NgTemplateOutlet],
  templateUrl: './partner-card.html',
  styleUrl: './partner-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PartnerCard {
  protected readonly text = inject(LanguageService).text;

  readonly partner = input.required<PartnerDto>();

  protected readonly logo = computed(() => partnerLogoSrc(this.partner()));
  protected readonly domain = computed(() => displayDomain(this.partner().websiteUrl));
}
