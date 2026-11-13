import { PartnerDto } from '../../../core/models';

/**
 * Bundled logos for the known partners, used until a logo is uploaded in the CMS.
 * Matched on the organisation's short name or website.
 */
const FALLBACK_LOGOS: readonly { readonly pattern: RegExp; readonly src: string }[] = [
  { pattern: /btu|ბტუ/i, src: 'brand/partners/btu.png' },
  { pattern: /atsu|აწსუ|წერეთ/i, src: 'brand/partners/atsu.svg' },
  { pattern: /rustaveli|srnsf|რუსთაველ/i, src: 'brand/partners/srnsfg.png' },
];

export function partnerLogoSrc(partner: PartnerDto, onDark = false): string | null {
  const uploaded = onDark ? (partner.logoOnDark ?? partner.logo) : partner.logo;
  if (uploaded) {
    return uploaded.url;
  }
  const haystack = `${partner.shortName} ${partner.name} ${partner.websiteUrl ?? ''}`;
  return FALLBACK_LOGOS.find((logo) => logo.pattern.test(haystack))?.src ?? null;
}
