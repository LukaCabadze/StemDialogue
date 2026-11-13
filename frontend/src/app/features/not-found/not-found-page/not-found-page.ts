import { ChangeDetectionStrategy, Component, effect, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { Icon } from '../../../shared/ui/icon/icon';

/** Shown for unknown URLs inside the site chrome. */
@Component({
  selector: 'sd-not-found-page',
  imports: [RouterLink, Icon],
  templateUrl: './not-found-page.html',
  styleUrl: './not-found-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFoundPage {
  private readonly seo = inject(SeoService);
  protected readonly text = inject(LanguageService).text;
  protected readonly project = inject(ProjectContext);

  constructor() {
    effect(() => this.seo.apply({ title: this.text().common.notFoundTitle }));
  }
}
