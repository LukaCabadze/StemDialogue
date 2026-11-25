import { journeyLayout } from './partner-journey.path';

describe('partner journey path', () => {
  it('scales the two26-style curve uniformly to the track width', () => {
    const layout = journeyLayout(3, 1616, false);
    expect(layout.d.startsWith('M 1601 15 C')).toBe(true);
    expect(layout.nodes).toEqual([
      { x: 1218, y: 166 },
      { x: 513, y: 476 },
      { x: 846, y: 786 },
    ]);
    expect(layout.height).toBe(786 + 150);

    const half = journeyLayout(3, 808, false);
    expect(half.nodes[1]).toEqual({ x: 256.5, y: 238 });
  });

  it('stacks mirrored copies when there are more partners than nodes', () => {
    const layout = journeyLayout(8, 1616, false);
    expect(layout.nodes).toHaveLength(8);
    expect(layout.nodes[7]).toEqual({ x: 1616 - 1218, y: 166 + 2150 });
    expect(layout.d.match(/ L /g)?.length).toBe(1);
  });

  it('uses the narrow curve on phones', () => {
    const layout = journeyLayout(2, 407, true);
    expect(layout.nodes).toEqual([
      { x: 93, y: 186 },
      { x: 346, y: 441 },
    ]);
  });
});
