import { Injectable, inject } from '@angular/core';

import { LanguageService } from './language.service';
import { DateParts, pad2, toTbilisiParts } from './tbilisi-date';

export type DateFormat =
  /** 8 ნოემბერი, 2026 */
  | 'long'
  /** 8 ნოემბერი */
  | 'dayMonth'
  /** 08 */
  | 'day'
  /** ნოე */
  | 'monthShort'
  /** ნოემბერი 2026 */
  | 'monthYear'
  /** 12:00 */
  | 'time'
  /** 2026 */
  | 'year'
  /** 20 ნოემბრამდე */
  | 'until';

/** Formats API dates in Tbilisi time with the site's own month names (matching the design exactly). */
@Injectable({ providedIn: 'root' })
export class DateFormatService {
  private readonly language = inject(LanguageService);

  format(value: string | null | undefined, format: DateFormat): string {
    const parts = toTbilisiParts(value);
    return parts ? this.formatParts(parts, format) : '';
  }

  /** "12:00 – 13:30" when an end time is known. */
  timeRange(start: string | null | undefined, end: string | null | undefined): string {
    const from = this.format(start, 'time');
    const to = this.format(end, 'time');
    return from && to && from !== to ? `${from} – ${to}` : from;
  }

  /** "ოქტომბერი 2026 – დღემდე" for involvement periods. */
  period(since: string | null | undefined, until: string | null | undefined, presentLabel: string): string {
    const from = this.format(since, 'monthYear');
    const to = until ? this.format(until, 'monthYear') : presentLabel;
    return from ? `${from} – ${to}` : to === presentLabel ? '' : to;
  }

  private formatParts(parts: DateParts, format: DateFormat): string {
    const text = this.language.text().date;
    const isGeorgian = this.language.isGeorgian();
    const month = text.months[parts.month] ?? '';

    switch (format) {
      case 'long':
        return isGeorgian ? `${parts.day} ${month}, ${parts.year}` : `${parts.day} ${month} ${parts.year}`;
      case 'dayMonth':
        return `${parts.day} ${month}`;
      case 'day':
        return pad2(parts.day);
      case 'monthShort':
        return text.monthsShort[parts.month] ?? '';
      case 'monthYear':
        return `${month} ${parts.year}`;
      case 'time':
        return `${pad2(parts.hour)}:${pad2(parts.minute)}`;
      case 'year':
        return String(parts.year);
      case 'until':
        return `${parts.day} ${text.monthsUntil[parts.month] ?? month}`;
    }
  }
}
