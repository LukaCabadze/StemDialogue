import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { isExternalUrl } from '../../../core/util/external-url';
import { Icon } from '../icon/icon';

export type ActionLinkVariant = 'primary' | 'secondary' | 'text';

/**
 * Button-styled link for CMS actions whose URL may be internal (`/projects/...`) or external
 * (`https://...`). External links open in a new tab and get the ↗ icon.
 */
@Component({
  selector: 'sd-action-link',
  imports: [RouterLink, Icon],
  templateUrl: './action-link.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
})
export class ActionLink {
  protected readonly text = inject(LanguageService).text;

  readonly url = input.required<string>();
  readonly label = input.required<string>();
  readonly variant = input<ActionLinkVariant>('primary');
  readonly block = input(false);

  protected readonly external = computed(() => isExternalUrl(this.url()));
  protected readonly classes = computed(() => {
    const variant = this.variant();
    if (variant === 'text') {
      return 'sd-text-link';
    }
    return `sd-btn sd-btn--${variant}${this.block() ? ' sd-btn--block' : ''}`;
  });
}
