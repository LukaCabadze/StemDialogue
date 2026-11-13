/** Calendar parts of an instant as seen in Tbilisi (all public times are Asia/Tbilisi). */
export interface DateParts {
  readonly year: number;
  /** 0-based month index. */
  readonly month: number;
  readonly day: number;
  readonly hour: number;
  readonly minute: number;
}

export const SITE_TIME_ZONE = 'Asia/Tbilisi';

const DATE_ONLY = /^(\d{4})-(\d{2})-(\d{2})$/;

const formatter = new Intl.DateTimeFormat('en-GB', {
  timeZone: SITE_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
});

/** Returns null for empty or unparseable input so templates can simply skip the value. */
export function toTbilisiParts(value: string | null | undefined): DateParts | null {
  if (!value) {
    return null;
  }

  // A plain calendar date has no time zone; keep it as written.
  const dateOnly = DATE_ONLY.exec(value);
  if (dateOnly) {
    return {
      year: Number(dateOnly[1]),
      month: Number(dateOnly[2]) - 1,
      day: Number(dateOnly[3]),
      hour: 0,
      minute: 0,
    };
  }

  const instant = new Date(value);
  if (Number.isNaN(instant.getTime())) {
    return null;
  }

  const parts = Object.fromEntries(formatter.formatToParts(instant).map((part) => [part.type, part.value]));
  return {
    year: Number(parts['year']),
    month: Number(parts['month']) - 1,
    day: Number(parts['day']),
    hour: Number(parts['hour']),
    minute: Number(parts['minute']),
  };
}

export function pad2(value: number): string {
  return value.toString().padStart(2, '0');
}
