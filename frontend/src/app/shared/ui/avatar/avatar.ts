import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { MediaDto } from '../../../core/models';
import { initialsOf } from '../../../core/util/initials';

export type AvatarTone = 'gradient' | 'white';

/** Round portrait: the photo when available, otherwise initials on the brand gradient. */
@Component({
  selector: 'sd-avatar',
  templateUrl: './avatar.html',
  styleUrl: './avatar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.sd-avatar--white]': "tone() === 'white'",
    '[style.--sd-avatar-size.px]': 'size()',
  },
})
export class Avatar {
  readonly name = input.required<string>();
  readonly photo = input<MediaDto | null>(null);
  readonly size = input(40);
  readonly tone = input<AvatarTone>('gradient');

  protected readonly initials = computed(() => initialsOf(this.name()));
  /** Initials scale with the circle: 14px at 40, 22px at 72, 26px at 88, 44px at 160. */
  protected readonly fontSize = computed(() => Math.round(this.size() * 0.3));
}
