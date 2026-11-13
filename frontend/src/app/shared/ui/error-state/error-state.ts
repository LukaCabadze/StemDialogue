import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';

import { apiErrorKind } from '../../../core/http/api-error';
import { LanguageService } from '../../../core/i18n/language.service';
import { ProjectContext } from '../../../core/project/project-context.service';
import { Icon } from '../icon/icon';

/** Failed load: "not found" (404) offers the way home, anything else offers a retry. */
@Component({
  selector: 'sd-error-state',
  imports: [RouterLink, Icon],
  templateUrl: './error-state.html',
  styleUrl: './error-state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'alert' },
})
export class ErrorState {
  protected readonly text = inject(LanguageService).text;
  protected readonly homePath = inject(ProjectContext).rootPath;

  readonly error = input<unknown>(null);
  readonly retry = output<void>();

  protected readonly notFound = computed(() => apiErrorKind(this.error()) === 'not-found');
}
