/** The three illustrated placeholder covers from the design. */
export type CoverVariant = 'blue' | 'pink' | 'light';

const VARIANTS: readonly CoverVariant[] = ['blue', 'pink', 'light'];

/** Stable variant for a slug, so a card keeps its colour across visits. */
export function coverVariantFor(seed: string | null | undefined, offset = 0): CoverVariant {
  let hash = 0;
  for (const character of seed ?? '') {
    hash = (hash * 31 + character.charCodeAt(0)) >>> 0;
  }
  return VARIANTS[(hash + offset) % VARIANTS.length] ?? 'blue';
}

/** Rotates through the variants by position (used for card rows). */
export function coverVariantAt(index: number): CoverVariant {
  return VARIANTS[index % VARIANTS.length] ?? 'blue';
}
