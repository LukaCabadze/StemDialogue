import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Icon } from '../icon/icon';
import { IconName } from '../icon/icon-paths';

/** Dashed soft box: "materials will be published after the meeting", "former members", empty lists. */
@Component({
  selector: 'sd-empty-state',
  imports: [RouterLink, Icon],
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyState {
  readonly icon = input<IconName>('file-text');
  readonly title = input.required<string>();
  readonly text = input<string | null>(null);
  readonly linkLabel = input<string | null>(null);
  readonly linkPath = input<string | null>(null);
}
