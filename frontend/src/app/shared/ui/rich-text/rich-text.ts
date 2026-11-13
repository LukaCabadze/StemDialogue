import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * CMS HTML body. The API sanitises on save and Angular's `[innerHTML]` sanitiser runs again here,
 * so scripts, inline handlers and unknown embeds never reach the page.
 */
@Component({
  selector: 'sd-rich-text',
  template: `<div class="sd-prose" [innerHTML]="html()"></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: block' },
})
export class RichText {
  readonly html = input.required<string>();
}
