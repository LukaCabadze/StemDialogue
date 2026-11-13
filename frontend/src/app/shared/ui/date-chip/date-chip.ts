import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { SdDatePipe } from '../../pipes/sd-date.pipe';

export type DateChipSize = 'md' | 'lg';

/** Blue-tint calendar tile: Sora day number over the short month (52px or 64px). */
@Component({
  selector: 'sd-date-chip',
  imports: [SdDatePipe],
  templateUrl: './date-chip.html',
  styleUrl: './date-chip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.size-md]': "size() === 'md'",
  },
})
export class DateChip {
  protected readonly language = inject(LanguageService).language;

  readonly date = input.required<string>();
  readonly size = input<DateChipSize>('lg');
}
