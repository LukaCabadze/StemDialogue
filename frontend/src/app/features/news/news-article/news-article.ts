import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { NewsDetailDto } from '../../../core/models';
import { coverVariantFor } from '../../../core/util/cover-variant';
import { readingMinutes } from '../../../core/util/reading-time';
import { SdDatePipe } from '../../../shared/pipes/sd-date.pipe';
import { CoverArt } from '../../../shared/ui/cover-art/cover-art';
import { GalleryGrid } from '../../../shared/ui/gallery-grid/gallery-grid';
import { Pill } from '../../../shared/ui/pill/pill';
import { RichText } from '../../../shared/ui/rich-text/rich-text';
import { ShareButtons } from '../../../shared/ui/share-buttons/share-buttons';

/** Article body: category + date + reading time, title, lead, cover with caption, body, gallery, tags, share. */
@Component({
  selector: 'sd-news-article',
  imports: [SdDatePipe, CoverArt, GalleryGrid, Pill, RichText, ShareButtons],
  templateUrl: './news-article.html',
  styleUrl: './news-article.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NewsArticle {
  private readonly languages = inject(LanguageService);
  protected readonly text = this.languages.text;
  protected readonly language = this.languages.language;

  readonly article = input.required<NewsDetailDto>();

  protected readonly minutes = computed(() => readingMinutes(this.article().bodyHtml));
  protected readonly coverVariant = computed(() => coverVariantFor(this.article().slug));
  protected readonly categoryTone = computed(() => (this.article().category === 'EventSummary' ? 'pink' : 'blue'));
  protected readonly caption = computed(() => {
    const cover = this.article().cover;
    if (!cover) {
      return null;
    }
    const parts = [cover.caption, cover.credit ? `${this.text().news.photo}: ${cover.credit}` : null];
    return parts.filter((part) => !!part).join(' · ') || null;
  });
}
