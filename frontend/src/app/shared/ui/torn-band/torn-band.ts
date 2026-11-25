import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type TornBandTone = 'night' | 'blue' | 'pink';

/**
 * Full-width colour band with ripped-paper top and bottom edges, laid over the notebook-grid page.
 * Wrap any section in it: `<sd-torn-band tone="night"> … </sd-torn-band>`.
 */
@Component({
  selector: 'sd-torn-band',
  templateUrl: './torn-band.html',
  styleUrl: './torn-band.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': "'tone-' + tone()",
  },
})
export class TornBand {
  readonly tone = input<TornBandTone>('night');
}
