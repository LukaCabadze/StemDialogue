import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { MediaDto } from '../../../core/models';
import { CoverVariant } from '../../../core/util/cover-variant';
import { Icon } from '../icon/icon';

/**
 * Card/hero cover. Shows the uploaded image when there is one; otherwise draws the illustrated
 * brand placeholder (gradient, orbit rings, dots and the atom/symbol mark) from the design.
 * The host is sized by its parent; the art always fills it.
 */
@Component({
  selector: 'sd-cover-art',
  imports: [Icon],
  templateUrl: './cover-art.html',
  styleUrl: './cover-art.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': "'cover--' + variant()",
  },
})
export class CoverArt {
  readonly variant = input<CoverVariant>('blue');
  readonly image = input<MediaDto | null>(null);
  /** Small caption in the bottom-left corner (e.g. "ჩანაწერი · 1:24:10"). */
  readonly label = input<string | null>(null);
  /** Large white play button in the centre (recordings). */
  readonly showPlay = input(false);
  /** Hide the corner mark on very small tiles. */
  readonly showMark = input(true);
  /** Decorative by default; pass alt text only when the cover is meaningful content. */
  readonly alt = input('');
}
