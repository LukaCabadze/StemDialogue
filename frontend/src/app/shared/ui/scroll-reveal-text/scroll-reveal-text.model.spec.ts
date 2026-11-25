import { WAVE, glyphStyle, parseReveal, plainText } from './scroll-reveal-text.model';

describe('scroll reveal text', () => {
  it('splits words into indexed letters and finds the tagged word', () => {
    const words = parseReveal('STEM {ელჩები}, ჩვენ');
    expect(words.map((word) => word.tagged)).toEqual([false, true, false]);
    expect(words[1]!.glyphs.map((glyph) => glyph.char).join('')).toBe('ელჩები,');
    expect(words[2]!.glyphs[0]!.index).toBe(4 + 7);
  });

  it('removes the tag braces for screen readers', () => {
    expect(plainText('STEM {ambassadors} carry')).toBe('STEM ambassadors carry');
  });

  it('colours letters behind the wave as ink and ahead of it as light', () => {
    expect(glyphStyle(10, 10 + WAVE).color).toBe('rgb(20 43 75)');
    expect(glyphStyle(10, 5).color).toBe('rgb(198 214 245)');
    expect(glyphStyle(10, 10 + WAVE * 0.2).transform).not.toBe('none');
  });
});
