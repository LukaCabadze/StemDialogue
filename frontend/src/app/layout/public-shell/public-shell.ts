import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { LanguageService } from '../../core/i18n/language.service';
import { FundingStrip } from '../funding-strip/funding-strip';
import { SiteFooter } from '../site-footer/site-footer';
import { SiteHeader } from '../site-header/site-header';

/** Layout of every public project page: skip link, header, funding strip, page outlet, footer. */
@Component({
  selector: 'sd-public-shell',
  imports: [RouterOutlet, FundingStrip, SiteFooter, SiteHeader],
  templateUrl: './public-shell.html',
  styleUrl: './public-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicShell {
  protected readonly text = inject(LanguageService).text;
}
