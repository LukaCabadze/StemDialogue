import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { EventStatus } from '../../../core/models';

/** Planned → Held → Archive progress for one event page (one URL across the whole lifecycle). */
@Component({
  selector: 'sd-lifecycle-stepper',
  templateUrl: './lifecycle-stepper.html',
  styleUrl: './lifecycle-stepper.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LifecycleStepper {
  protected readonly text = inject(LanguageService).text;

  readonly status = input.required<EventStatus>();
  /** True once the recording or materials are published. */
  readonly hasArchive = input(false);

  protected readonly steps = computed(() => {
    const copy = this.text().events;
    return [copy.lifecyclePlanned, copy.lifecycleHeld, copy.lifecycleArchive];
  });

  protected readonly current = computed(() => {
    if (this.status() !== 'Completed') {
      return 0;
    }
    return this.hasArchive() ? 2 : 1;
  });
}
