import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';

export interface Crumb {
  readonly label: string;
  /** Omit for the current page (last crumb). */
  readonly path?: string | null;
}

/** "მთავარი / სიახლეები / **title**" trail above page headings. */
@Component({
  selector: 'sd-breadcrumbs',
  imports: [RouterLink],
  templateUrl: './breadcrumbs.html',
  styleUrl: './breadcrumbs.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Breadcrumbs {
  protected readonly text = inject(LanguageService).text;

  readonly items = input.required<readonly Crumb[]>();
}
