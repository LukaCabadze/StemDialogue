import { initialsOf } from './initials';

describe('initialsOf', () => {
  it('takes the first letter of the first two words', () => {
    expect(initialsOf('გვანცა წულაია')).toBe('გწ');
    expect(initialsOf('Saba  Gela Shvili')).toBe('SG');
  });

  it('handles missing names', () => {
    expect(initialsOf(null)).toBe('');
    expect(initialsOf('   ')).toBe('');
  });
});
