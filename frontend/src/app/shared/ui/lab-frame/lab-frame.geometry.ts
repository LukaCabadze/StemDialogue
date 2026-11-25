/**
 * Pure geometry for `<sd-lab-frame>`: a glass tube that leaves an Erlenmeyer flask on the left,
 * runs over the top of the content and drops into a round-bottom flask on the right.
 * Everything is built in real pixels from the measured frame size, so nothing is stretched.
 */

export interface Point {
  readonly x: number;
  readonly y: number;
}

export interface Bubble {
  readonly cx: number;
  readonly cy: number;
  readonly r: number;
  /** Animation delay in seconds. */
  readonly delay: number;
}

export interface Flask {
  /** Closed glass outline, also used as the liquid clip path. */
  readonly body: string;
  readonly rim: { readonly x: number; readonly y: number; readonly width: number; readonly height: number };
  /** Back and front liquid waves (wider than the flask so they can slide). */
  readonly back: string;
  readonly front: string;
  readonly bubbles: readonly Bubble[];
  /** Little side arm of the round-bottom flask. */
  readonly arm: { readonly x1: number; readonly y1: number; readonly x2: number; readonly y2: number } | null;
  /** Drop that bobs next to the neck. */
  readonly drop: { readonly cx: number; readonly cy: number; readonly r: number } | null;
}

export interface LabFrameShape {
  readonly width: number;
  readonly height: number;
  readonly viewBox: string;
  readonly tube: string;
  readonly left: Flask;
  readonly right: Flask;
  readonly band: { readonly back: string; readonly front: string };
}

/** Wavelengths must match the CSS animation distances in lab-frame.scss. */
export const WAVE_A = 60;
export const WAVE_B = 84;
export const BAND_BACK = 360;
export const BAND_FRONT = 280;

/** Below this width the frame is hidden and the content uses the normal page layout. */
export const MIN_FRAME_WIDTH = 1200;
const MIN_FRAME_HEIGHT = 520;

// Must match `.content` padding in lab-frame.scss and `.sd-container` in _layout.scss.
const CONTAINER = 1200;
const GUTTER = 24;
/** Side padding that keeps the flasks clear of the content on narrower screens. */
const SIDE_PAD = 146;

const TOP = 22;
const DIP = 50;
const CORNER = 30;
const SIDE_OFFSET = 86;
const FLASK_MARGIN = 84;

const round = (value: number): number => Math.round(value * 10) / 10;

/** Polyline with arcs at every corner (radius clamped to half of each neighbouring segment). */
export function roundedPath(points: readonly Point[], radius: number): string {
  if (points.length < 2) {
    return '';
  }
  const first = points[0]!;
  let d = `M ${round(first.x)} ${round(first.y)}`;
  for (let index = 1; index < points.length - 1; index++) {
    const prev = points[index - 1]!;
    const current = points[index]!;
    const next = points[index + 1]!;
    const inLength = Math.hypot(current.x - prev.x, current.y - prev.y);
    const outLength = Math.hypot(next.x - current.x, next.y - current.y);
    if (inLength === 0 || outLength === 0) {
      continue;
    }
    const ix = (current.x - prev.x) / inLength;
    const iy = (current.y - prev.y) / inLength;
    const ox = (next.x - current.x) / outLength;
    const oy = (next.y - current.y) / outLength;
    const cross = ix * oy - iy * ox;
    if (Math.abs(cross) < 1e-6) {
      d += ` L ${round(current.x)} ${round(current.y)}`;
      continue;
    }
    const r = Math.min(radius, inLength / 2, outLength / 2);
    d +=
      ` L ${round(current.x - ix * r)} ${round(current.y - iy * r)}` +
      ` A ${round(r)} ${round(r)} 0 0 ${cross > 0 ? 1 : 0} ${round(current.x + ox * r)} ${round(current.y + oy * r)}`;
  }
  const last = points[points.length - 1]!;
  return `${d} L ${round(last.x)} ${round(last.y)}`;
}

/**
 * Filled sine-like wave from `from` to `to` at height `level`, closed down to `bottom`.
 * It overhangs by one wavelength on both sides, so sliding it by exactly one wavelength loops seamlessly.
 */
export function wavePath(from: number, to: number, level: number, amplitude: number, wavelength: number, bottom: number): string {
  const half = wavelength / 2;
  const start = from - wavelength;
  const end = to + wavelength;
  let x = start + half;
  let d = `M ${round(start)} ${round(level)} Q ${round(start + half / 2)} ${round(level - amplitude)} ${round(x)} ${round(level)}`;
  while (x < end) {
    x += half;
    d += ` T ${round(x)} ${round(level)}`;
  }
  return `${d} L ${round(x)} ${round(bottom)} L ${round(start)} ${round(bottom)} Z`;
}

function erlenmeyer(cx: number, bottom: number): Flask & { readonly neckTop: number } {
  const neck = 18;
  const neckTop = bottom - 196;
  const shoulder = bottom - 128;
  const base = 64;
  const level = bottom - 62;
  const body =
    `M ${cx - neck} ${neckTop} V ${shoulder} L ${cx - base + 4} ${bottom - 18}` +
    ` Q ${cx - base - 2} ${bottom} ${cx - base + 18} ${bottom} H ${cx + base - 18}` +
    ` Q ${cx + base + 2} ${bottom} ${cx + base - 4} ${bottom - 18} L ${cx + neck} ${shoulder} V ${neckTop} Z`;
  return {
    neckTop,
    body,
    rim: { x: cx - neck - 9, y: neckTop - 12, width: neck * 2 + 18, height: 14 },
    back: wavePath(cx - base - 8, cx + base + 8, level - 5, 5, WAVE_A, bottom + 4),
    front: wavePath(cx - base - 8, cx + base + 8, level, 6, WAVE_B, bottom + 4),
    bubbles: [
      { cx: cx - 28, cy: bottom - 22, r: 4, delay: 0 },
      { cx: cx + 10, cy: bottom - 16, r: 6, delay: 0.9 },
      { cx: cx + 32, cy: bottom - 30, r: 3.5, delay: 1.7 },
      { cx: cx - 6, cy: bottom - 38, r: 5, delay: 2.3 },
    ],
    arm: null,
    drop: { cx: cx + neck + 17, cy: neckTop - 2, r: 8 },
  };
}

function roundBottom(cx: number, bottom: number): Flask & { readonly neckTop: number } {
  const radius = 74;
  const neck = 17;
  const cy = bottom - radius;
  const join = cy - Math.sqrt(radius * radius - neck * neck);
  const neckTop = join - 62;
  const level = cy + 10;
  return {
    neckTop,
    body: `M ${cx - neck} ${round(neckTop)} V ${round(join)} A ${radius} ${radius} 0 1 0 ${cx + neck} ${round(join)} V ${round(neckTop)} Z`,
    rim: { x: cx - neck - 9, y: round(neckTop - 12), width: neck * 2 + 18, height: 14 },
    back: wavePath(cx - radius - 8, cx + radius + 8, level - 5, 6, WAVE_A, bottom + 4),
    front: wavePath(cx - radius - 8, cx + radius + 8, level, 7, WAVE_B, bottom + 4),
    bubbles: [
      { cx: cx - 34, cy: bottom - 24, r: 5, delay: 0.4 },
      { cx: cx + 18, cy: bottom - 14, r: 4, delay: 1.3 },
      { cx: cx + 4, cy: bottom - 40, r: 6, delay: 2.1 },
      { cx: cx + 40, cy: bottom - 34, r: 3.5, delay: 0 },
    ],
    arm: { x1: cx - neck, y1: round(neckTop + 32), x2: cx - neck - 30, y2: round(neckTop + 6) },
    drop: null,
  };
}

/** Full frame for a box of `width` × `height` px, or null when there is no room for it. */
export function buildLabFrame(width: number, height: number): LabFrameShape | null {
  if (width < MIN_FRAME_WIDTH || height < MIN_FRAME_HEIGHT) {
    return null;
  }

  // Where the page content sits (same maths as the CSS padding + .sd-container).
  const content = Math.min(CONTAINER, width - SIDE_PAD * 2 - GUTTER * 2);
  const contentLeft = (width - content) / 2;
  const contentRight = contentLeft + content;

  const left = Math.max(FLASK_MARGIN, contentLeft - SIDE_OFFSET);
  const right = Math.min(width - FLASK_MARGIN, contentRight + SIDE_OFFSET);
  const floor = height - 16;
  const leftFlask = erlenmeyer(left, floor);
  const rightFlask = roundBottom(right, floor);

  const dip = TOP + DIP;
  const points: Point[] = [
    { x: left, y: leftFlask.neckTop + 8 },
    { x: left, y: TOP },
    { x: contentLeft + content * 0.2, y: TOP },
    { x: contentLeft + content * 0.2, y: dip },
    { x: contentLeft + content * 0.56, y: dip },
    { x: contentLeft + content * 0.56, y: TOP },
    { x: contentLeft + content * 0.68, y: TOP },
    { x: contentLeft + content * 0.68, y: dip },
    { x: right, y: dip },
  ];

  // A small outward loop on the right side, like a condenser bend, when there is room for it.
  const kink = Math.min(48, width - right - 24);
  const drop = rightFlask.neckTop - dip;
  if (kink >= 24 && drop >= 220) {
    const kinkTop = dip + drop * 0.5 - 32;
    points.push(
      { x: right, y: kinkTop },
      { x: right + kink, y: kinkTop },
      { x: right + kink, y: kinkTop + 64 },
      { x: right, y: kinkTop + 64 },
    );
  }
  points.push({ x: right, y: rightFlask.neckTop + 8 });

  return {
    width,
    height,
    viewBox: `0 0 ${width} ${height}`,
    tube: roundedPath(points, CORNER),
    left: leftFlask,
    right: rightFlask,
    band: {
      back: wavePath(0, width, height - 46, 9, BAND_BACK, height + 2),
      front: wavePath(0, width, height - 30, 8, BAND_FRONT, height + 2),
    },
  };
}
