import { formatBytes, orNull, parseUtc, tbilisiInputToUtc, utcToTbilisiInput } from './admin-time';

describe('admin time helpers', () => {
  it('shows UTC instants as Tbilisi wall-clock time for datetime inputs', () => {
    expect(utcToTbilisiInput('2026-11-28T08:00:00Z')).toBe('2026-11-28T12:00');
    expect(utcToTbilisiInput('2026-11-28T22:30:00Z')).toBe('2026-11-29T02:30');
  });

  it('treats API timestamps without an offset as UTC', () => {
    expect(parseUtc('2026-11-28T08:00:00').toISOString()).toBe('2026-11-28T08:00:00.000Z');
  });

  it('converts Tbilisi input back to UTC, round-tripping exactly', () => {
    expect(tbilisiInputToUtc('2026-11-28T12:00')).toBe('2026-11-28T08:00:00.000Z');
    expect(tbilisiInputToUtc(utcToTbilisiInput('2027-05-15T06:45:00Z'))).toBe('2027-05-15T06:45:00.000Z');
  });

  it('returns empty/null for missing values', () => {
    expect(utcToTbilisiInput(null)).toBe('');
    expect(tbilisiInputToUtc('')).toBeNull();
    expect(tbilisiInputToUtc('nonsense')).toBeNull();
  });

  it('normalises optional text and formats sizes', () => {
    expect(orNull('  ')).toBeNull();
    expect(orNull(' a ')).toBe('a');
    expect(formatBytes(512)).toBe('512 B');
    expect(formatBytes(1536)).toBe('1.5 KB');
    expect(formatBytes(25 * 1024 * 1024)).toBe('25 MB');
  });
});
