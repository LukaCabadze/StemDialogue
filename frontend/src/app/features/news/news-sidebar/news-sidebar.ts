import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { CategoryCountDto, NewsDetailDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { DateChip } from '../../../shared/ui/date-chip/date-chip';
import { Icon } from '../../../shared/ui/icon/icon';
import { Pill } from '../../../shared/ui/pill/pill';
import { StatusPill } from '../../../shared/ui/status-pill/status-pill';

/** Article sidebar: the related meeting card and the category list (current one highlighted). */
@Component({
  selector: 'sd-news-sidebar',
  imports: [RouterLink, DateChip, Icon, Pill, StatusPill],
  templateUrl: './news-sidebar.html',
  styleUrl: './news-sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'sd-sidebar' },
})
export class NewsSidebar {
  private readonly project = inject(ProjectContext);
  protected readonly text = inject(LanguageService).text;

  readonly article = input.required<NewsDetailDto>();
  readonly categories = input<readonly CategoryCountDto[]>([]);

  protected readonly listPath = computed(() => this.project.path('news'));
}
