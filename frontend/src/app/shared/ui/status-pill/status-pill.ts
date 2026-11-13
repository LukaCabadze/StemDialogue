import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { EventStatus } from '../../../core/models';

/** Event lifecycle badge with its status colour and dot (planned / postponed / completed / cancelled). */
@Component({
  selector: 'sd-status-pill',
  template: `<span class="dot" aria-hidden="true"></span>{{ label() }}`,
  styleUrl: './status-pill.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': "'status--' + status().toLowerCase()",
  },
})
export class StatusPill {
  readonly status = input.required<EventStatus>();
  readonly label = input.required<string>();
}
