import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { LanguageService } from '../../core/i18n/language.service';
import { Language } from '../../core/models';

interface LanguageOption {
  readonly value: Language;
  readonly short: string;
  readonly name: string;
}

/** "ქარ · EN" toggle in the header. */
@Component({
  selector: 'sd-language-switch',
  templateUrl: './language-switch.html',
  styleUrl: './language-switch.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LanguageSwitch {
  protected readonly languages = inject(LanguageService);

  protected readonly options: readonly LanguageOption[] = [
    { value: 'ka', short: 'ქარ', name: 'ქართული' },
    { value: 'en', short: 'EN', name: 'English' },
  ];
}
