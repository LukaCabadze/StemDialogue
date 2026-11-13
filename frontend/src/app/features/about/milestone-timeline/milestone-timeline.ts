import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { MilestoneDto } from '../../../core/models';

/** Vertical project timeline; done steps are filled, the current one is highlighted. */
@Component({
  selector: 'sd-milestone-timeline',
  templateUrl: './milestone-timeline.html',
  styleUrl: './milestone-timeline.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MilestoneTimeline {
  protected readonly text = inject(LanguageService).text;

  readonly milestones = input.required<readonly MilestoneDto[]>();
}
