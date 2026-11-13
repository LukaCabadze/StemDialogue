/** True for absolute http(s) links that should open in a new tab. */
export function isExternalUrl(url: string | null | undefined): boolean {
  return !!url && /^https?:\/\//i.test(url);
}

/** Domain shown under partner logos: "https://www.btu.edu.ge/" → "btu.edu.ge". */
export function displayDomain(url: string | null | undefined): string {
  if (!url) {
    return '';
  }
  try {
    return new URL(url).hostname.replace(/^www\./, '');
  } catch {
    return url;
  }
}

const EMBED_HOSTS = ['www.youtube.com', 'youtube.com', 'www.youtube-nocookie.com', 'player.vimeo.com', 'www.facebook.com'];

/** Only well-known video players may be embedded as iframes. */
export function isTrustedEmbedUrl(url: string | null | undefined): url is string {
  if (!url) {
    return false;
  }
  try {
    const parsed = new URL(url);
    return parsed.protocol === 'https:' && EMBED_HOSTS.includes(parsed.hostname);
  } catch {
    return false;
  }
}
