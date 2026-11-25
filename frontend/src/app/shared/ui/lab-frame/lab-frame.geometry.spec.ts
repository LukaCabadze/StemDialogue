import { buildLabFrame, roundedPath, wavePath } from './lab-frame.geometry';

describe('lab frame geometry', () => {
  it('rounds every corner of a polyline with an arc', () => {
    const d = roundedPath(
      [
        { x: 0, y: 100 },
        { x: 0, y: 0 },
        { x: 100, y: 0 },
      ],
      20,
    );
    expect(d).toBe('M 0 100 L 0 20 A 20 20 0 0 1 20 0 L 100 0');
  });

  it('builds a closed wave that overhangs one wavelength on each side', () => {
    const d = wavePath(0, 100, 50, 5, 40, 80);
    expect(d.startsWith('M -40 50')).toBe(true);
    expect(d.endsWith('Z')).toBe(true);
  });

  it('skips the frame when there is no room for it', () => {
    expect(buildLabFrame(1100, 900)).toBeNull();
    expect(buildLabFrame(1400, 300)).toBeNull();
  });

  it('keeps both flasks on screen and clear of the content at 1440px', () => {
    const frame = buildLabFrame(1440, 900)!;
    const contentLeft = 146 + 24;
    expect(frame.left.rim.x).toBeGreaterThan(0);
    expect(frame.left.rim.x + frame.left.rim.width).toBeLessThan(contentLeft);
    expect(frame.right.rim.x + frame.right.rim.width).toBeLessThan(1440);
  });

  it('keeps the flasks outside the content column on wide screens', () => {
    const frame = buildLabFrame(1800, 900)!;
    const contentLeft = (1800 - 1200) / 2;
    expect(frame.left.rim.x + frame.left.rim.width).toBeLessThan(contentLeft);
    expect(frame.viewBox).toBe('0 0 1800 900');
  });
});
