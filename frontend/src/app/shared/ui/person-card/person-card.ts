import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { MediaDto } from '../../../core/models';
import { Avatar } from '../avatar/avatar';

/** Portrait card used on the team page and the researcher / mentor directories. */
@Component({
  selector: 'sd-person-card',
  imports: [RouterLink, Avatar],
  templateUrl: './person-card.html',
  styleUrl: './person-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonCard {
  readonly name = input.required<string>();
  readonly photo = input<MediaDto | null>(null);
  /** Pink label above the name ("პროექტის ხელმძღვანელი"). */
  readonly role = input<string | null>(null);
  /** Muted line under the name (title · organisation). */
  readonly description = input<string | null>(null);
  readonly path = input<string | null>(null);
}
