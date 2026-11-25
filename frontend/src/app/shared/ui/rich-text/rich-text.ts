import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * CMS HTML body. The API sanitises on save and Angular's `[innerHTML]` sanitiser runs again here,
 * so scripts, inline handlers and unknown embeds never reach the page.
 */
@Component({
  selector: 'sd-rich-text',
  template: `<div class="sd-prose" [class.sd-prose--large]="size() === 'large'" [innerHTML]="html()"></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: block' },
})
export class RichText {
  readonly html = input.required<string>();
  /** `large` sets the body as a big statement (used next to the about timeline). */
  readonly size = input<'body' | 'large'>('body');
}
