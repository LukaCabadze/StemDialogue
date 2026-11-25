/** Pure helpers for `<sd-scroll-reveal-text>`: text parsing and per-letter colours. */

export interface Glyph {
  readonly char: string;
  /** Position of the letter in the whole text (spaces excluded). */
  readonly index: number;
}

export interface RevealWord {
  readonly glyphs: readonly Glyph[];
  /** The floating tag is pinned to this word (written as `{word}` in the text). */
  readonly tagged: boolean;
}

export interface GlyphStyle {
  readonly color: string;
  readonly transform: string;
}

/** How many letters the colour wave is spread across (bright edge → dark ink). */
export const WAVE = 16;

// Light (unread) → bright edge → brand blue → ink, all in our palette.
const LIGHT: Rgb = [198, 214, 245];
const EDGE: Rgb = [59, 123, 255];
const BLUE: Rgb = [8, 69, 181];
const INK: Rgb = [20, 43, 75];

type Rgb = readonly [number, number, number];

const READ: GlyphStyle = { color: rgb(INK), transform: 'none' };
const UNREAD: GlyphStyle = { color: rgb(LIGHT), transform: 'none' };

/** Splits the text into words and letters; `{word}` marks the tagged word. */
export function parseReveal(text: string): readonly RevealWord[] {
  let index = 0;
  return text
    .split(/\s+/)
    .filter((word) => word.length > 0)
    .map((raw) => {
      const tagged = /^\{.+\}[.,!?;:]*$/.test(raw);
      const word = tagged ? raw.replace(/[{}]/g, '') : raw;
      const glyphs = Array.from(word, (char) => ({ char, index: index++ }));
      return { glyphs, tagged };
    });
}

/** Plain text for screen readers (tag braces removed). */
export function plainText(text: string): string {
  return text.replace(/[{}]/g, '');
}

/**
 * Style of one letter. `reach` is how many letters the wave has passed (0 … total + WAVE).
 * Letters behind the wave are ink, letters ahead are light, and the ones inside it glow blue
 * and lift slightly, like the reading line on two26.studio.
 */
export function glyphStyle(index: number, reach: number): GlyphStyle {
  const distance = reach - index;
  if (distance <= 0) {
    return UNREAD;
  }
  if (distance >= WAVE) {
    return READ;
  }
  const t = distance / WAVE;
  const color =
    distance < 1
      ? mix(LIGHT, EDGE, distance)
      : t < 0.4
        ? mix(EDGE, BLUE, t / 0.4)
        : mix(BLUE, INK, (t - 0.4) / 0.6);

  const lift = t < 0.6 ? Math.sin((Math.PI * t) / 0.6) : 0;
  const transform =
    lift > 0.01 ? `translateY(${(-0.06 * lift).toFixed(3)}em) scale(${(1 + 0.08 * lift).toFixed(3)})` : 'none';
  return { color, transform };
}

/** Static style for visitors who prefer reduced motion. */
export const STATIC_GLYPH = READ;

function mix(from: Rgb, to: Rgb, amount: number): string {
  const t = Math.min(1, Math.max(0, amount));
  return rgb([
    from[0] + (to[0] - from[0]) * t,
    from[1] + (to[1] - from[1]) * t,
    from[2] + (to[2] - from[2]) * t,
  ]);
}

function rgb(value: Rgb): string {
  return `rgb(${Math.round(value[0])} ${Math.round(value[1])} ${Math.round(value[2])})`;
}
