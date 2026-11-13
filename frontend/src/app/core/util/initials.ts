/**
 * "გვანცა წულაია" → "გწ". Uses the first letter of the first two words. Only Latin letters are
 * capitalised: upper-casing Georgian would turn Mkhedruli into Mtavruli, which the design does not use.
 */
export function initialsOf(fullName: string | null | undefined): string {
  if (!fullName) {
    return '';
  }
  return fullName
    .trim()
    .split(/\s+/)
    .filter((word) => word.length > 0)
    .slice(0, 2)
    .map((word) => Array.from(word)[0] ?? '')
    .join('')
    .replace(/[a-z]/g, (letter) => letter.toUpperCase());
}
