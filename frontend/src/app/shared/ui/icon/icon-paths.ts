/**
 * Stroke icons (24×24, 2px, round caps) used across the site. Kept as static SVG markup so the
 * icon component can render them without an icon font or network requests.
 */
export const ICON_PATHS = {
  'arrow-right': '<path d="M5 12h14"/><path d="m13 6 6 6-6 6"/>',
  'arrow-left': '<path d="M19 12H5"/><path d="m11 18-6-6 6-6"/>',
  'arrow-up-right': '<path d="M7 17 17 7"/><path d="M8 7h9v9"/>',
  'external-link':
    '<path d="M15 3h6v6"/><path d="M10 14 21 3"/><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/>',
  'chevron-down': '<path d="m6 9 6 6 6-6"/>',
  calendar:
    '<rect x="3" y="4.5" width="18" height="16.5" rx="2.5"/><path d="M16 2.5v4"/><path d="M8 2.5v4"/><path d="M3 10h18"/>',
  'calendar-plus':
    '<rect x="3" y="4.5" width="18" height="16.5" rx="2.5"/><path d="M16 2.5v4"/><path d="M8 2.5v4"/><path d="M3 10h18"/><path d="M12 13.5v5"/><path d="M9.5 16h5"/>',
  'calendar-alert':
    '<rect x="3" y="4.5" width="18" height="16.5" rx="2.5"/><path d="M16 2.5v4"/><path d="M8 2.5v4"/><path d="M3 10h18"/><path d="M12 13v3"/><path d="M12 18.5h.01"/>',
  clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
  video: '<path d="m16 13 5.2 3.1a.5.5 0 0 0 .8-.4V8.3a.5.5 0 0 0-.8-.4L16 11"/><rect x="2" y="6" width="14" height="12" rx="2"/>',
  play: '<path d="M7 4.5v15a.8.8 0 0 0 1.2.7l12-7.5a.8.8 0 0 0 0-1.4l-12-7.5A.8.8 0 0 0 7 4.5Z"/>',
  atom:
    '<circle cx="12" cy="12" r="1.2"/><path d="M20.2 20.2c2-2-.1-7.3-4.6-11.8S5.8 1.8 3.8 3.8s.1 7.3 4.6 11.8 9.8 6.6 11.8 4.6Z"/><path d="M15.6 15.6c4.5-4.5 6.6-9.8 4.6-11.8S12.9 3.9 8.4 8.4 1.8 18.2 3.8 20.2s7.3-.1 11.8-4.6Z"/>',
  'file-text':
    '<path d="M14.5 2.5H6.8a2 2 0 0 0-2 2v15a2 2 0 0 0 2 2h10.4a2 2 0 0 0 2-2V7.2Z"/><path d="M14 2.5v5h5"/><path d="M9 13h6"/><path d="M9 17h6"/>',
  presentation:
    '<path d="M2.5 3.5h19"/><path d="M4 3.5v10a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-10"/><path d="m8 21 4-5.5 4 5.5"/>',
  image:
    '<rect x="3" y="3" width="18" height="18" rx="2.5"/><circle cx="9" cy="9" r="2"/><path d="m21 15-3.1-3.1a2 2 0 0 0-2.8 0L6 21"/>',
  link: '<path d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7"/><path d="M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7"/>',
  download: '<path d="M12 3v12"/><path d="m7 10 5 5 5-5"/><path d="M5 21h14"/>',
  target: '<circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="5"/><circle cx="12" cy="12" r="1"/>',
  users:
    '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9"/><path d="M16 3.1a4 4 0 0 1 0 7.8"/>',
  compass: '<circle cx="12" cy="12" r="9"/><path d="m15.8 8.2-2.1 5.5-5.5 2.1 2.1-5.5Z"/>',
  'map-pin': '<path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/><circle cx="12" cy="10" r="3"/>',
  mail: '<rect x="2.5" y="4.5" width="19" height="15" rx="2.5"/><path d="m3 7 9 6 9-6"/>',
  phone:
    '<path d="M21.5 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 1.6 4.2 2 2 0 0 1 3.6 2h3a2 2 0 0 1 2 1.7c.1 1 .4 1.9.7 2.8a2 2 0 0 1-.5 2.1L7.6 9.8a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.8.6 2.8.7a2 2 0 0 1 1.7 2Z"/>',
  menu: '<path d="M4 6h16"/><path d="M4 12h16"/><path d="M4 18h16"/>',
  close: '<path d="M18 6 6 18"/><path d="m6 6 12 12"/>',
  search: '<circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/>',
  sparkle: '<path d="M12 3v4"/><path d="M12 17v4"/><path d="M3 12h4"/><path d="M17 12h4"/><path d="m6.3 6.3 2.1 2.1"/><path d="m15.6 15.6 2.1 2.1"/><path d="m6.3 17.7 2.1-2.1"/><path d="m15.6 8.4 2.1-2.1"/>',
  school: '<path d="M22 10 12 5 2 10l10 5 10-5Z"/><path d="M6 12v5c3 2.5 9 2.5 12 0v-5"/>',
  archive: '<rect x="2.5" y="3.5" width="19" height="5" rx="1.5"/><path d="M4.5 8.5v10a2 2 0 0 0 2 2h11a2 2 0 0 0 2-2v-10"/><path d="M10 12.5h4"/>',
  check: '<path d="M20 6 9 17l-5-5"/>',
  globe: '<circle cx="12" cy="12" r="9"/><path d="M3 12h18"/><path d="M12 3a14 14 0 0 1 0 18 14 14 0 0 1 0-18Z"/>',
} as const;

export type IconName = keyof typeof ICON_PATHS;
