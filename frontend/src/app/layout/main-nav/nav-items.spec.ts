import { UI_TEXT_KA } from '../../core/i18n/ui-text.ka';
import { buildNavigation } from './nav-items';

describe('buildNavigation', () => {
  const root = '/projects/school2university';

  it('groups team, researchers and mentors under "people"', () => {
    const entries = buildNavigation(root, UI_TEXT_KA, null);
    const people = entries.find((entry) => entry.key === 'people');

    expect(people?.kind).toBe('group');
    expect(people?.kind === 'group' ? people.children.map((child) => child.path) : []).toEqual([
      `${root}/team`,
      `${root}/researchers`,
      `${root}/mentors`,
    ]);
  });

  it('uses CMS labels and hides pages the CMS leaves out of navigation', () => {
    const entries = buildNavigation(root, UI_TEXT_KA, [
      { key: 'news', label: 'ახალი ამბები', path: `${root}/news` },
      { key: 'team', label: 'გუნდი', path: `${root}/team` },
    ]);

    expect(entries.map((entry) => entry.label)).toEqual(['ახალი ამბები', UI_TEXT_KA.nav.people]);
  });
});
