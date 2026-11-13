/** Google Calendar "add event" link for a meeting. */
export function googleCalendarUrl(options: {
  readonly title: string;
  readonly startsAt: string;
  readonly endsAt?: string | null;
  readonly details?: string | null;
  readonly location?: string | null;
}): string | null {
  const start = new Date(options.startsAt);
  if (Number.isNaN(start.getTime())) {
    return null;
  }
  const end = options.endsAt ? new Date(options.endsAt) : new Date(start.getTime() + 90 * 60 * 1000);
  const stamp = (date: Date) => date.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '');

  const params = new URLSearchParams({
    action: 'TEMPLATE',
    text: options.title,
    dates: `${stamp(start)}/${stamp(end)}`,
  });
  if (options.details) {
    params.set('details', options.details);
  }
  if (options.location) {
    params.set('location', options.location);
  }
  return `https://calendar.google.com/calendar/render?${params.toString()}`;
}
