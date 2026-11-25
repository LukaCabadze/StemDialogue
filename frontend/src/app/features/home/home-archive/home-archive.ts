import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { ArchiveHighlightsDto } from '../../../core/models';
import { SdDatePipe } from '../../../shared/pipes/sd-date.pipe';
import { CoverArt } from '../../../shared/ui/cover-art/cover-art';
import { ResourceItem } from '../../../shared/ui/resource-item/resource-item';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';
import { StatusPill } from '../../../shared/ui/status-pill/status-pill';
import { TornBand } from '../../../shared/ui/torn-band/torn-band';

/** "04 · Archive" on the dark band: the latest recording and a list of recent materials. */
@Component({
  selector: 'sd-home-archive',
  imports: [RouterLink, SdDatePipe, CoverArt, ResourceItem, SectionHeader, StatusPill, TornBand],
  templateUrl: './home-archive.html',
  styleUrl: './home-archive.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeArchive {
  private readonly languages = inject(LanguageService);
  protected readonly text = this.languages.text;
  protected readonly language = this.languages.language;

  readonly archive = input.required<ArchiveHighlightsDto>();
  readonly archivePath = input.required<string>();
  readonly index = input('04');

  protected readonly archiveQuery = { scope: 'Past' } as const;
  protected readonly highlight = computed(() => this.archive().recentEvents[0] ?? null);
}
