import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { NewsCardDto } from '../../../core/models';
import { coverVariantAt } from '../../../core/util/cover-variant';
import { NewsCard } from '../../../shared/ui/news-card/news-card';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';

/** "03 · News": the three latest articles (placeholders alternate blue / pink / light). */
@Component({
  selector: 'sd-home-news',
  imports: [NewsCard, SectionHeader],
  templateUrl: './home-news.html',
  styleUrl: './home-news.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeNews {
  protected readonly text = inject(LanguageService).text;
  protected readonly variantAt = coverVariantAt;

  readonly news = input.required<readonly NewsCardDto[]>();
  readonly listPath = input.required<string>();
  readonly index = input('03');
}
