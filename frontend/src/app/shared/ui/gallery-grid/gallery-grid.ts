import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { MediaDto } from '../../../core/models';

const VISIBLE = 5;

/** Photo mosaic: one large tile + four small ones; the last tile shows "+N" for the rest. */
@Component({
  selector: 'sd-gallery-grid',
  templateUrl: './gallery-grid.html',
  styleUrl: './gallery-grid.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GalleryGrid {
  readonly items = input.required<readonly MediaDto[]>();
  readonly label = input.required<string>();

  protected readonly visible = computed(() => this.items().slice(0, VISIBLE));
  protected readonly hiddenCount = computed(() => Math.max(0, this.items().length - VISIBLE));
}
