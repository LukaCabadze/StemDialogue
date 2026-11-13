/** Rough reading time for an HTML body (~180 words per minute, minimum one minute). */
export function readingMinutes(html: string | null | undefined): number {
  if (!html) {
    return 1;
  }
  const words = html
    .replace(/<[^>]+>/g, ' ')
    .split(/\s+/)
    .filter((word) => word.length > 0).length;
  return Math.max(1, Math.round(words / 180));
}
