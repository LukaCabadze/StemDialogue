import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type PillTone = 'outline' | 'blue' | 'pink' | 'dark';

/** Small 28px label: format/tag (outline), category (blue / pink tint). */
@Component({
  selector: 'sd-pill',
  template: `
    @if (dot()) {
      <span class="dot" aria-hidden="true"></span>
    }
    <ng-content />
  `,
  styleUrl: './pill.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': "'sd-pill sd-pill--' + tone()",
  },
})
export class Pill {
  readonly tone = input<PillTone>('outline');
  readonly dot = input(false);
}
