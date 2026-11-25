// Editors enter times in Tbilisi time (UTC+4, no daylight saving since 2005); the API stores UTC.

const TBILISI_OFFSET_MS = 4 * 60 * 60 * 1000;

/** The API returns UTC; values without an offset are treated as UTC too. */
export function parseUtc(value: string): Date {
  return new Date(/[zZ]|[+-]\d\d:?\d\d$/.test(value) ? value : `${value}Z`);
}

/** UTC → `YYYY-MM-DDTHH:mm` for `<input type="datetime-local">` in Tbilisi time. */
export function utcToTbilisiInput(utc: string | null | undefined): string {
  if (!utc) {
    return '';
  }
  return new Date(parseUtc(utc).getTime() + TBILISI_OFFSET_MS).toISOString().slice(0, 16);
}

/** `YYYY-MM-DDTHH:mm` (Tbilisi) → ISO UTC, or null when empty. */
export function tbilisiInputToUtc(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }
  const asUtc = new Date(`${value.length === 16 ? `${value}:00` : value}Z`);
  return Number.isNaN(asUtc.getTime()) ? null : new Date(asUtc.getTime() - TBILISI_OFFSET_MS).toISOString();
}

const dateTimeFormat = new Intl.DateTimeFormat('ka-GE', {
  timeZone: 'Asia/Tbilisi',
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

const dateFormat = new Intl.DateTimeFormat('ka-GE', { year: 'numeric', month: 'short', day: 'numeric' });

export function formatUtc(utc: string | null | undefined): string {
  return utc ? dateTimeFormat.format(parseUtc(utc)) : '—';
}

export function formatDate(date: string | null | undefined): string {
  return date ? dateFormat.format(new Date(`${date}T00:00:00`)) : '—';
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }
  const units = ['KB', 'MB', 'GB'];
  let value = bytes / 1024;
  let unit = 0;
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024;
    unit++;
  }
  return `${value.toFixed(value < 10 ? 1 : 0)} ${units[unit]}`;
}

/** Empty strings from inputs become null for optional API fields. */
export function orNull(value: string | null | undefined): string | null {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
