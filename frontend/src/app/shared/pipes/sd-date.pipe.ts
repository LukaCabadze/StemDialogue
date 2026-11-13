import { Pipe, PipeTransform, inject } from '@angular/core';

import { DateFormat, DateFormatService } from '../../core/i18n/date-format.service';
import { Language } from '../../core/models';

/**
 * `{{ event.startsAt | sdDate: 'long' : language() }}` — the language argument is only a change
 * trigger so the pure pipe re-runs after the visitor switches language.
 */
@Pipe({ name: 'sdDate' })
export class SdDatePipe implements PipeTransform {
  private readonly dates = inject(DateFormatService);

  transform(value: string | null | undefined, format: DateFormat = 'long', _language?: Language): string {
    return this.dates.format(value, format);
  }
}
