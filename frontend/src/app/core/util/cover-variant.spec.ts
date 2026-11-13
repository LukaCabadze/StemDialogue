import { coverVariantAt, coverVariantFor } from './cover-variant';

describe('cover variants', () => {
  it('is stable for the same slug', () => {
    expect(coverVariantFor('robots-around-us')).toBe(coverVariantFor('robots-around-us'));
  });

  it('rotates blue → pink → light by position', () => {
    expect([0, 1, 2, 3].map(coverVariantAt)).toEqual(['blue', 'pink', 'light', 'blue']);
  });
});
