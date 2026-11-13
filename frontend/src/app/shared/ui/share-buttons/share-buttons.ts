import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';

import { APP_ENVIRONMENT } from '../../../core/config/app-environment.token';
import { LanguageService } from '../../../core/i18n/language.service';
import { Icon } from '../icon/icon';

/** Facebook / LinkedIn share links and a copy-link button for the current page. */
@Component({
  selector: 'sd-share-buttons',
  imports: [Icon],
  templateUrl: './share-buttons.html',
  styleUrl: './share-buttons.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShareButtons {
  private readonly document = inject(DOCUMENT);
  private readonly siteUrl = inject(APP_ENVIRONMENT).siteUrl.replace(/\/+$/, '');
  protected readonly text = inject(LanguageService).text;

  /** Site-relative path of the shared page (from the API's `path`). */
  readonly path = input.required<string>();
  readonly showLabel = input(false);

  protected readonly copied = signal(false);
  protected readonly url = computed(() => `${this.siteUrl}${this.path()}`);
  protected readonly facebookUrl = computed(
    () => `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(this.url())}`,
  );
  protected readonly linkedInUrl = computed(
    () => `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(this.url())}`,
  );

  protected async copy(): Promise<void> {
    try {
      await this.document.defaultView?.navigator.clipboard.writeText(this.url());
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {
      this.copied.set(false);
    }
  }
}
