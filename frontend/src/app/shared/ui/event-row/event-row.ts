import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { EventCardDto } from '../../../core/models';
import { DateChip } from '../date-chip/date-chip';
import { Icon } from '../icon/icon';
import { StatusPill } from '../status-pill/status-pill';

/** Compact event line (date tile · status + title · link), used in profile and sidebar lists. */
@Component({
  selector: 'sd-event-row',
  imports: [RouterLink, DateChip, Icon, StatusPill],
  templateUrl: './event-row.html',
  styleUrl: './event-row.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventRow {
  readonly event = input.required<EventCardDto>();
  readonly linkLabel = input.required<string>();
}
