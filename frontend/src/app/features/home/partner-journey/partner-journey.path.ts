/**
 * Path geometry for the partner journey, modelled on the services line of two26.studio: one smooth,
 * hand-drawn curve (loops, wide swings) that is scaled uniformly to the page width, with the
 * partner logos sitting on fixed points of that curve. Mobile has its own taller, narrower curve.
 *
 * Both curves use only absolute `M` / `C` commands, so they can be scaled, mirrored and stacked.
 */

export interface Point {
  readonly x: number;
  readonly y: number;
}

interface Template {
  /** Drawing width / height in its own units. */
  readonly width: number;
  readonly height: number;
  readonly d: string;
  /** Where the nodes sit on the curve, top to bottom. */
  readonly nodes: readonly Point[];
  /** Extra drawing below the last node before the track ends. */
  readonly tail: number;
  /** Vertical offset between stacked copies (only used for more partners than nodes). */
  readonly period: number;
}

const DESKTOP: Template = {
  width: 1616,
  height: 2187,
  d:
    'M1601 15C1241.24 56.6913 1025.14 308.977 1139.71 377.393C1191.45 408.29 1240.74 339.444 1181.39 282.059' +
    'C1075.45 179.627 937.582 215.243 741.458 298.287C701.638 315.147 622.383 350.59 552.298 410.201' +
    'C487.298 465.486 487.725 549.018 534.765 563.77C581.804 578.523 632.714 579.634 681.058 587.392' +
    'C729.731 595.202 767.011 617.17 797.836 657.756C833.766 705.065 838.864 772.034 862.127 825.767' +
    'C890.491 891.283 966.341 911.2 1033.5 915.638C1130.56 922.052 1195.74 891.215 1286.16 899.069' +
    'C1347.7 904.414 1457.75 932.208 1466.92 1059.42C1477.81 1210.68 1340.4 1252.99 1221.76 1286.05' +
    'C1118.2 1314.91 997.775 1397.54 862.127 1334.5C757.468 1285.86 666.411 1293.34 650.401 1384.21' +
    'C634.391 1475.07 710.968 1532.13 837.283 1542.81C963.598 1553.5 1146.44 1534.93 1181.39 1676' +
    'C1226.53 1858.21 954.075 1839.5 797.836 1839.5C661.891 1839.5 422.283 1807.72 385.5 1700' +
    'C340.427 1568 635.939 1584.37 465.5 1917C414.516 2016.5 310 2165.31 15 2165.31',
  nodes: [
    { x: 1218, y: 166 },
    { x: 513, y: 476 },
    { x: 846, y: 786 },
    { x: 1476, y: 1096 },
    { x: 667, y: 1406 },
    { x: 1193, y: 1717 },
    { x: 366, y: 2027 },
  ],
  tail: 150,
  period: 2150,
};

const MOBILE: Template = {
  width: 407,
  height: 1556,
  d:
    'M542.705 10C465.727 48.981 92.8237 79.5116 92.8237 185.592C92.8237 291.673 352.712 336 346.322 440.868' +
    'C339.933 545.736 104.278 543.251 92.8237 652.131C79.6013 777.82 353.287 765.564 346.322 898.5' +
    'C339.358 1031.44 63.9084 911.5 68 1046C72.0916 1180.5 335.245 1170.41 328.309 1279.59' +
    'C320.5 1402.5 -37.4522 1513.46 -135.704 1545.09',
  nodes: [
    { x: 93, y: 186 },
    { x: 346, y: 441 },
    { x: 93, y: 652 },
    { x: 346, y: 899 },
    { x: 68, y: 1046 },
    { x: 328, y: 1280 },
  ],
  tail: 240,
  period: 1535,
};

export interface JourneyLayout {
  /** Path in pixels. */
  readonly d: string;
  /** One node per partner, in pixels. */
  readonly nodes: readonly Point[];
  /** Track height in pixels. */
  readonly height: number;
  /** Stroke width in pixels (scaled like the original: 16 units desktop, 20 units mobile). */
  readonly stroke: number;
}

/**
 * Lays the curve out for `count` partners on a track `width` px wide. More partners than the
 * curve has nodes are handled by stacking mirrored copies of the curve below each other.
 */
export function journeyLayout(count: number, width: number, narrow: boolean): JourneyLayout {
  const template = narrow ? MOBILE : DESKTOP;
  const scale = width / template.width;
  const copies = Math.max(1, Math.ceil(count / template.nodes.length));

  const place = (point: Point, copy: number): Point => {
    const mirrored = copy % 2 === 1;
    return {
      x: round((mirrored ? template.width - point.x : point.x) * scale),
      y: round((point.y + copy * template.period) * scale),
    };
  };

  const nodes: Point[] = [];
  for (let index = 0; index < count; index++) {
    const copy = Math.floor(index / template.nodes.length);
    nodes.push(place(template.nodes[index % template.nodes.length]!, copy));
  }

  const pairs = parsePairs(template.d);
  let d = '';
  for (let copy = 0; copy < copies; copy++) {
    const points = pairs.map((pair) => place(pair, copy));
    // Copy 0 starts with a move; later copies continue the line from where the previous one ended.
    const [start, ...curves] = points;
    d += copy === 0 ? `M ${start!.x} ${start!.y}` : ` L ${start!.x} ${start!.y}`;
    for (let index = 0; index + 2 < curves.length; index += 3) {
      const [c1, c2, end] = [curves[index]!, curves[index + 1]!, curves[index + 2]!];
      d += ` C ${c1.x} ${c1.y}, ${c2.x} ${c2.y}, ${end.x} ${end.y}`;
    }
  }

  const last = nodes[nodes.length - 1] ?? { x: 0, y: 0 };
  return {
    d,
    nodes,
    height: round(last.y + template.tail * scale),
    stroke: round((narrow ? 20 : 16) * scale * (narrow ? 1 : 1.1)),
  };
}

/** All coordinate pairs of an absolute M/C path, in order. */
function parsePairs(d: string): Point[] {
  const numbers = (d.match(/-?\d*\.?\d+/g) ?? []).map(Number);
  const pairs: Point[] = [];
  for (let index = 0; index + 1 < numbers.length; index += 2) {
    pairs.push({ x: numbers[index]!, y: numbers[index + 1]! });
  }
  return pairs;
}

function round(value: number): number {
  return Math.round(value * 10) / 10;
}
