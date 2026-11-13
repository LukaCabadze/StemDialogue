import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

import { ICON_PATHS, IconName } from './icon-paths';

/**
 * Inline stroke icon. Decorative by default (`aria-hidden`); pass `label` when the icon
 * carries meaning on its own.
 */
@Component({
  selector: 'sd-icon',
  templateUrl: './icon.html',
  styleUrl: './icon.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[style.width.px]': 'size()',
    '[style.height.px]': 'size()',
  },
})
export class Icon {
  private readonly sanitizer = inject(DomSanitizer);

  readonly name = input.required<IconName>();
  readonly size = input(18);
  readonly strokeWidth = input(2);
  readonly filled = input(false);
  readonly label = input<string | null>(null);

  // The markup is a compile-time constant from ICON_PATHS, never user input.
  protected readonly markup = computed<SafeHtml>(() =>
    this.sanitizer.bypassSecurityTrustHtml(ICON_PATHS[this.name()]),
  );
}
