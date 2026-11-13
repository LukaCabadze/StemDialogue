import { toTbilisiParts } from './tbilisi-date';

describe('toTbilisiParts', () => {
  it('reads API timestamps in Tbilisi time regardless of the browser zone', () => {
    expect(toTbilisiParts('2026-11-21T08:00:00Z')).toEqual({ year: 2026, month: 10, day: 21, hour: 12, minute: 0 });
  });

  it('keeps plain calendar dates as written', () => {
    expect(toTbilisiParts('2026-10-01')).toEqual({ year: 2026, month: 9, day: 1, hour: 0, minute: 0 });
  });

  it('returns null for empty or invalid input', () => {
    expect(toTbilisiParts(null)).toBeNull();
    expect(toTbilisiParts('not a date')).toBeNull();
  });
});
