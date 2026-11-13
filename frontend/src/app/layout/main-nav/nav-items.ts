import { NavItemDto } from '../../core/models';
import { UiText } from '../../core/i18n/ui-text.model';

export interface NavLink {
  readonly kind: 'link';
  readonly key: string;
  readonly label: string;
  readonly path: string;
}

export interface NavGroup {
  readonly kind: 'group';
  readonly key: string;
  readonly label: string;
  readonly children: readonly NavLink[];
}

export type NavEntry = NavLink | NavGroup;

const PEOPLE_KEYS = ['team', 'researchers', 'mentors'] as const;
const ORDER = ['about', 'stem-saturdays', 'news', 'people', 'stem-ambassadors', 'resources', 'contact'] as const;

/**
 * Builds the header menu. Labels and visibility come from the CMS navigation when it is loaded;
 * team, researchers and mentors are grouped under "ადამიანები".
 */
export function buildNavigation(
  rootPath: string,
  text: UiText,
  cmsNavigation: readonly NavItemDto[] | null,
): readonly NavEntry[] {
  const fallbackLabels: Readonly<Record<string, string>> = {
    about: text.nav.about,
    'stem-saturdays': text.nav.stemSaturdays,
    news: text.nav.news,
    team: text.nav.team,
    researchers: text.nav.researchers,
    mentors: text.nav.mentors,
    'stem-ambassadors': text.nav.ambassadors,
    resources: text.nav.resources,
    contact: text.nav.contact,
  };

  const visible = (key: string) => !cmsNavigation?.length || cmsNavigation.some((item) => item.key === key);
  const link = (key: string): NavLink => ({
    kind: 'link',
    key,
    label: cmsNavigation?.find((item) => item.key === key)?.label ?? fallbackLabels[key] ?? key,
    path: `${rootPath}/${key}`,
  });

  const entries: NavEntry[] = [];
  for (const key of ORDER) {
    if (key === 'people') {
      const children = PEOPLE_KEYS.filter(visible).map(link);
      if (children.length > 0) {
        entries.push({ kind: 'group', key, label: text.nav.people, children });
      }
    } else if (visible(key)) {
      entries.push(link(key));
    }
  }
  return entries;
}
